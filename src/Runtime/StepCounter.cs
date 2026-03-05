
/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Step counter implementation
 *  Date Updated : 
 *
 *************************************************************/


using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Enums;
using Vexit.FlowEngine.Models;

/// <summary>
/// Helper class to count steps without executing them.
/// </summary>
public class StepCounter : IFlowController
{
    private readonly IActivityRegistry _registry;

    public StepCounter(IActivityRegistry registry)
    {
        _registry = registry;
    }

    public int StepCount { get; private set; }

    public FlowInstance Instance => throw new NotImplementedException();

    public Task Step(string activityKey, object? inputs = null)
    {
        var activityType = _registry.GetActivityType(activityKey) ?? ActivityTypeSE.Step;
        if (activityType.EnumValue == ActivityTypeEnum.Step)
        {
            StepCount++;
        }
        return Task.CompletedTask;
    }

    public Task Group(string groupName, Func<Task> actions)
    {
        return actions();
    }

    public Task GroupParallel(string groupName, Func<Task> actions)
    {
        return actions();
    }

    public Task Decide(Func<bool> condition, Func<Task> then, Func<Task>? @else = null)
    {
        // Count the then branch
        if (condition())
        {
            return then();
        }
        else if (@else != null)
        {
            return @else();
        }
        return Task.CompletedTask;
    }

}
