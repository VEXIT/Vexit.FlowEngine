/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Flow orchestrator implementation
 *  Date Updated : 2026-01-28 | Vex | Added input resolution and output mapping, discovery integration, context-merge input resolution and namespaced outputs, flow instance state persistence
 *                 2026-01-29 | Vex | Added flow storage configuration - It allows flows to override application-wide storage configuration
 *                 2026-01-31 | Vex | Added activity observers and step-only progress tracking
 *                 2026-02-05 | Vex | Added runtime storage configuration override capability via RunAsync parameter
 *
 *************************************************************/

using System.Reflection;
using System.Text.Json;
using Vexit.Common.Models;
using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Core.Attributes;
using Vexit.FlowEngine.Discovery.Registry;
using Vexit.FlowEngine.Discovery.Scanners;
using Vexit.FlowEngine.Enums;
using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Runtime;

/// <summary>
/// Concrete implementation of IFlowRunner and IFlowController.
/// Manages execution, state tracking, and activity dispatch.
/// </summary>
public sealed class FlowOrchestrator : IFlowRunner, IFlowController
{

    private readonly IActivityRegistry _activityRegistry;
    private readonly FlowRegistry _flowRegistry;
    private readonly ActivityScanner _activityScanner;
    private readonly FlowScanner _flowScanner;
    private readonly FlowEngineConfig _options;
    private readonly IInputResolver _inputResolver;
    private readonly IStateStore _stateStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<IFlowObserver> _observers;
    private bool _discovered;
    private int _totalSteps;
    private int _completedSteps;

    public FlowInstance Instance { get; private set; } = new() { FlowDefinitionId = string.Empty };




    //-----------------------------------------------------------------
    //      CONSTRUCTOR
    //-----------------------------------------------------------------

    public FlowOrchestrator(
        IActivityRegistry activityRegistry,
        FlowRegistry flowRegistry,
        ActivityScanner activityScanner,
        FlowScanner flowScanner,
        FlowEngineConfig options,
        IInputResolver inputResolver,
        IStateStore stateStore,
        IServiceProvider serviceProvider,
        IEnumerable<IFlowObserver>? observers = null)
    {
        _activityRegistry = activityRegistry ?? throw new ArgumentNullException(nameof(activityRegistry));
        _flowRegistry = flowRegistry ?? throw new ArgumentNullException(nameof(flowRegistry));
        _activityScanner = activityScanner ?? throw new ArgumentNullException(nameof(activityScanner));
        _flowScanner = flowScanner ?? throw new ArgumentNullException(nameof(flowScanner));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _inputResolver = inputResolver ?? throw new ArgumentNullException(nameof(inputResolver));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _observers = observers?.ToList() ?? new List<IFlowObserver>();
    }

    /// <summary>
    /// Runs a new flow instance using the provided context.
    /// </summary>
    public async Task<Result> RunAsync<TFlow, TContext>(string instanceId, TContext context)
        where TFlow : class, IFlow<TContext>
        where TContext : class
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            throw new ArgumentException("InstanceId must be provided.", nameof(instanceId));
        }


        EnsureDiscovery(typeof(TFlow).Assembly);

        Instance = CreateFlowInstance<TFlow>(instanceId, context);

        // Count total steps for progress tracking
        _totalSteps = CountTotalSteps<TFlow, TContext>(context);
        _completedSteps = 0;
        var flowStartTime = DateTime.UtcNow;

        NotifyFlowStarted(Instance.FlowDefinitionId, Instance.Id, _totalSteps);

        await _stateStore.SaveFlowInstanceAsync(Instance);

        try
        {
            var flow = CreateFlow<TFlow>();
            await flow.Build(this, context);

            Instance.Status = "Completed";
            Instance.CompletedAt = DateTime.UtcNow;
            await _stateStore.SaveFlowInstanceAsync(Instance);

            var flowDuration = DateTime.UtcNow - flowStartTime;
            NotifyFlowCompleted(Instance.FlowDefinitionId, Instance.Id, flowDuration);

            return Result.Success();
        }
        catch (Exception ex)
        {
            Instance.Status = "Failed";
            Instance.CompletedAt = DateTime.UtcNow;
            await _stateStore.SaveFlowInstanceAsync(Instance);
            NotifyFlowFailed(Instance.FlowDefinitionId, Instance.Id, ex);

            return Result.Error(ex);
        }
    }

    /// <summary>
    /// Resumes a flow instance from the persisted state store.
    /// </summary>
    public async Task<Result> ResumeAsync<TFlow, TContext>(string flowInstanceId)
        where TFlow : class, IFlow<TContext>
        where TContext : class
    {
        EnsureDiscovery(typeof(TFlow).Assembly);

        var instance = await _stateStore.LoadFlowInstanceAsync(flowInstanceId);
        if (instance == null)
        {
            return Result.Failure($"Flow instance '{flowInstanceId}' not found.");
        }

        Instance = instance;
        Instance.Status = "Running";
        await _stateStore.SaveFlowInstanceAsync(Instance);

        try
        {
            var flow = CreateFlow<TFlow>();
            var context = CreateContextFromDictionary<TContext>(Instance.Context);

            await flow.Build(this, context);

            Instance.Status = "Completed";
            Instance.CompletedAt = DateTime.UtcNow;
            await _stateStore.SaveFlowInstanceAsync(Instance);

            return Result.Success();
        }
        catch (Exception ex)
        {
            Instance.Status = "Failed";
            Instance.CompletedAt = DateTime.UtcNow;
            await _stateStore.SaveFlowInstanceAsync(Instance);

            return Result.Error(ex);
        }
    }

    /// <summary>
    /// Executes a single activity by key. Skips if already completed.
    /// </summary>
    public async Task Step(string activityKey, object? inputs = null)
    {
        if (string.IsNullOrWhiteSpace(activityKey))
        {
            throw new ArgumentException("Activity key must be provided.", nameof(activityKey));
        }

        if (IsActivityCompleted(activityKey))
        {
            var skippedType = _activityRegistry.GetActivityType(activityKey) ?? ActivityTypeSE.Step;
            if (skippedType.EnumValue == ActivityTypeEnum.Step)
            {
                _completedSteps++;
            }
            await NotifyActivitySkipped(activityKey, skippedType, _completedSteps, _totalSteps);
            return;
        }

        var outputHandler = _activityRegistry.GetWithOutputs(activityKey);
        var basicHandler = _activityRegistry.GetBasic(activityKey);
        if (outputHandler == null && basicHandler == null)
        {
            throw new InvalidOperationException($"Activity '{activityKey}' is not registered.");
        }

        var inputPayload = ResolveInputs(activityKey, inputs);
        var activityInstance = new ActivityInstance
        {
            ActivityDefinitionId = activityKey,
            Status = ActivityStatusEnum.Running,
            StartedAt = DateTime.UtcNow,
            Inputs = ConvertInputsToDictionary(inputPayload ?? new { })
        };

        try
        {
            var activityType = _activityRegistry.GetActivityType(activityKey) ?? ActivityTypeSE.Step;
            var activityState = ActivityState.FromActivityInstance(activityInstance);
            NotifyActivityStarted(activityKey, activityState, _completedSteps + 1, _totalSteps);

            if (outputHandler != null)
            {
                var result = await outputHandler(inputPayload);
                activityInstance.Status = result.IsSuccess ? ActivityStatusEnum.Completed : ActivityStatusEnum.Failed;
                activityInstance.StatusMessage = result.IsSuccess ? null : result.Message;

                // Store outputs in activity instance
                if (result.IsSuccess && result.Data != null)
                {
                    foreach (var (key, value) in result.Data)
                    {
                        activityInstance.Outputs[key] = value;
                    }
                }

                // Map outputs to flow context
                MapOutputsToContext(activityKey, result, activityInstance);
            }
            else
            {
                var result = await basicHandler!(inputPayload);
                activityInstance.Status = result.IsSuccess ? ActivityStatusEnum.Completed : ActivityStatusEnum.Failed;
                activityInstance.StatusMessage = result.IsSuccess ? null : result.Message;
            }
        }
        catch (InvalidOperationException)
        {
            // Re-throw observer exceptions to stop the flow
            throw;
        }
        catch (Exception ex)
        {
            activityInstance.Status = ActivityStatusEnum.Failed;
            activityInstance.StatusMessage = ex.Message;
            throw; // Re-throw activity exceptions to stop the flow
        }
        finally
        {
            activityInstance.CompletedAt = DateTime.UtcNow;
            Instance.CurrentStepId = activityKey;
            Instance.CompletedSteps.Add(activityInstance);

            await _stateStore.SaveFlowInstanceAsync(Instance);

            var completedType = _activityRegistry.GetActivityType(activityKey) ?? ActivityTypeSE.Step;
            if (completedType.EnumValue == ActivityTypeEnum.Step)
            {
                _completedSteps++;
            }
            var activityState = ActivityState.FromActivityInstance(activityInstance);
            NotifyActivityCompleted(activityKey, activityState, _completedSteps, _totalSteps, activityInstance.StatusMessage);
        }
    }

    /// <summary>
    /// Executes a sequential group of steps.
    /// </summary>
    public async Task Group(string groupName, Func<Task> actions)
    {
        if (actions == null) throw new ArgumentNullException(nameof(actions));
        await actions();
    }

    /// <summary>
    /// Executes a group intended for parallel steps (currently sequential).
    /// </summary>
    public async Task GroupParallel(string groupName, Func<Task> actions)
    {
        if (actions == null) throw new ArgumentNullException(nameof(actions));
        await actions();
    }

    /// <summary>
    /// Executes conditional branching logic for code-first flows.
    /// </summary>
    public async Task Decide(Func<bool> condition, Func<Task> then, Func<Task>? @else = null)
    {
        if (condition == null) throw new ArgumentNullException(nameof(condition));
        if (then == null) throw new ArgumentNullException(nameof(then));

        if (condition())
        {
            await then();
        }
        else if (@else != null)
        {
            await @else();
        }
    }

    /// <summary>
    /// Creates or resolves a flow instance via DI.
    /// </summary>
    private TFlow CreateFlow<TFlow>() where TFlow : class
    {
        var flow = _serviceProvider.GetService(typeof(TFlow)) as TFlow;
        return flow ?? Activator.CreateInstance<TFlow>();
    }

    /// <summary>
    /// Initializes a new flow instance and seeds context.
    /// </summary>
    private FlowInstance CreateFlowInstance<TFlow>(string id, object context)
    {
        var flowId = typeof(TFlow).GetCustomAttribute<FlowAttribute>()?.Id
                     ?? typeof(TFlow).Name;

        return new FlowInstance
        {
            Id = id,
            FlowDefinitionId = flowId,
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            Context = ConvertInputsToDictionary(context)
        };
    }

    /// <summary>
    /// Resolves inputs for an activity. Merges explicit inputs with flow context.
    /// </summary>
    private object? ResolveInputs(string activityKey, object? inputs)
    {
        var contextInputs = new Dictionary<string, object>(Instance.Context, StringComparer.OrdinalIgnoreCase);

        if (inputs == null)
        {
            // If no explicit inputs, pass context to allow parameter binding by name.
            return contextInputs;
        }

        var explicitInputs = ConvertInputsToDictionary(inputs);
        foreach (var (key, value) in contextInputs)
        {
            explicitInputs.TryAdd(key, value);
        }

        return explicitInputs;
    }

    /// <summary>
    /// Maps step outputs into the flow context using an activity-key namespace.
    /// </summary>
    private void MapOutputsToContext(string activityKey, Result<Dictionary<string, object>> result, ActivityInstance activityInstance)
    {
        if (!result.IsSuccess || result.Data == null)
        {
            return;
        }

        // Store outputs with activity-specific prefixes for namespacing
        foreach (var (key, value) in result.Data)
        {
            Instance.Context[$"{activityKey}.{key}"] = value;
        }
    }

    /// <summary>
    /// Performs one-time activity and flow discovery.
    /// </summary>
    private void EnsureDiscovery(Assembly fallbackAssembly)
    {
        if (_discovered)
        {
            return;
        }

        var assembly = _options.DiscoveryAssembly ?? fallbackAssembly;
        
        _activityScanner.Scan(assembly, _serviceProvider, _activityRegistry);
        _flowScanner.Scan(assembly, _flowRegistry);

        _discovered = true;
    }




    /// <summary>
    /// Counts total steps in a flow by building it without executing.
    /// </summary>
    private int CountTotalSteps<TFlow, TContext>(TContext context)
        where TFlow : class, IFlow<TContext>
        where TContext : class
    {
        var stepCounter = new StepCounter(_activityRegistry);
        var flow = CreateFlow<TFlow>();
        try
        {
            flow.Build(stepCounter, context);
        }
        catch
        {
            // Ignore any execution errors during counting
        }
        return stepCounter.StepCount;
    }

    private void NotifyFlowStarted(string flowId, string instanceId, int totalSteps)
    {
        foreach (var observer in _observers)
        {
            try
            {
                observer.OnFlowStarted(flowId, instanceId, totalSteps);
            }
            catch { }
        }
    }

    private void NotifyFlowCompleted(string flowId, string instanceId, TimeSpan duration)
    {
        foreach (var observer in _observers)
        {
            try
            {
                observer.OnFlowCompleted(flowId, instanceId, duration);
            }
            catch { }
        }
    }

    private void NotifyFlowFailed(string flowId, string instanceId, Exception ex)
    {
        foreach (var observer in _observers)
        {
            try
            {
                observer.OnFlowFailed(flowId, instanceId, ex);
            }
            catch { }
        }
    }

    private void NotifyActivityStarted(string activityKey, ActivityState activityState, int currentStep, int totalSteps)
    {
        var activityInfo = GetActivityInfo(activityKey);
        var progress = new FlowProgress
        {
            CompletedSteps = currentStep - 1, // currentStep is 1-based, so completed is currentStep - 1
            TotalSteps = totalSteps
        };

        foreach (var observer in _observers)
        {
            try
            {
                observer.OnActivityStarted(activityInfo, activityState, progress);
            }
            catch { }
        }
    }

    private void NotifyActivityCompleted(string activityKey, ActivityState activityState, int completedSteps, int totalSteps, string? errorMessage = null)
    {
        var activityInfo = GetActivityInfo(activityKey);

        var result = activityState.Status == ActivityStatusEnum.Completed
            ? Result<ActivityState>.Success(activityState)
            : Result<ActivityState>.Failure(errorMessage ?? "Activity failed");

        var progress = new FlowProgress
        {
            CompletedSteps = completedSteps,
            TotalSteps = totalSteps
        };

        Exception? observerException = null;
        foreach (var observer in _observers)
        {
            try
            {
                observer.OnActivityCompleted(activityInfo, result, progress);
            }
            catch (Exception ex)
            {
                observerException = ex;
                break;
            }
        }

        if (observerException != null)
        {
            throw observerException;
        }
    }

    private async Task NotifyActivitySkipped(string activityKey, ActivityTypeSE activityType, int completedSteps, int totalSteps)
    {
        var activityInfo = GetActivityInfo(activityKey);

        // Load the existing activity instance to get the real status
        var activityInstance = Instance.CompletedSteps
            .FirstOrDefault(a => a.ActivityDefinitionId == activityKey);

        var activityState = activityInstance != null
            ? ActivityState.FromActivityInstance(activityInstance)
            : new ActivityState
            {
                Status = ActivityStatusEnum.Skipped // Fallback if no instance found
            };

        var progress = new FlowProgress
        {
            CompletedSteps = completedSteps,
            TotalSteps = totalSteps
        };

        foreach (var observer in _observers)
        {
            try
            {
                observer.OnActivitySkipped(activityInfo, activityState, progress);
            }
            catch { }
        }
    }

    private string GetActivityName(string activityKey)
    {
        // First try to get the name from the activity registry (from attribute)
        var registeredName = _activityRegistry.GetActivityName(activityKey);
        if (!string.IsNullOrWhiteSpace(registeredName))
        {
            return registeredName;
        }

        // Fallback: Convert activity key to user-friendly display name
        // e.g., "server.setup.phase_core_system_setup" -> "Phase Core System Setup"
        var parts = activityKey.Split('.');
        if (parts.Length > 1)
        {
            var namePart = parts.Last();
            // Convert snake_case to Title Case
            var words = namePart.Split('_')
                .Select(word => string.IsNullOrEmpty(word) ? word : char.ToUpper(word[0]) + word.Substring(1).ToLower())
                .Where(word => !string.IsNullOrEmpty(word));

            return string.Join(" ", words);
        }
        return activityKey;
    }

    /// <summary>
    /// Gets activity information from the registry, with fallback name generation if needed.
    /// </summary>
    private ActivityInfo GetActivityInfo(string activityKey)
    {
        var activityInfo = _activityRegistry.GetActivityInfo(activityKey);

        if (activityInfo != null)
        {
            // If no name was set in the attribute, generate a fallback name
            if (string.IsNullOrEmpty(activityInfo.Name))
            {
                activityInfo.Name = GetActivityName(activityKey);
            }
            return activityInfo;
        }

        // Fallback if activity not found in registry (shouldn't happen in normal flow)
        return new ActivityInfo
        {
            Key = activityKey,
            Name = GetActivityName(activityKey),
            Type = ActivityTypeSE.Step
        };
    }


    //------------------------------------------------------------
    //      Utils
    //------------------------------------------------------------

    private static bool IsActivityCompleted(string activityKey, FlowInstance instance)
    {
        return instance.CompletedSteps.Any(step =>
            string.Equals(step.ActivityDefinitionId, activityKey, StringComparison.OrdinalIgnoreCase) &&
            step.Status == ActivityStatusEnum.Completed);
    }

    private bool IsActivityCompleted(string activityKey)
        => IsActivityCompleted(activityKey, Instance);

    private static Dictionary<string, object> ConvertInputsToDictionary(object inputs)
    {
        if (inputs is Dictionary<string, object> dictionary)
        {
            return new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
        }

        if (inputs is JsonElement element)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText())
                   ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var properties = inputs.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(inputs);
            if (value != null)
            {
                result[property.Name] = value;
            }
        }

        return result;
    }

    private static TContext CreateContextFromDictionary<TContext>(Dictionary<string, object> context)
        where TContext : class
    {
        var instance = Activator.CreateInstance<TContext>();
        var properties = typeof(TContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var property in properties)
        {
            if (!context.TryGetValue(property.Name, out var value) || value == null)
            {
                continue;
            }

            var converted = ConvertValue(value, property.PropertyType);
            if (converted != null)
            {
                property.SetValue(instance, converted);
            }
        }

        return instance;
    }

    private static object? ConvertValue(object value, Type targetType)
    {
        if (value is JsonElement jsonElement)
        {
            return JsonSerializer.Deserialize(jsonElement.GetRawText(), targetType);
        }

        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return null;
        }
    }
}

