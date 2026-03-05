/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Activity scanner for reflection discovery
 *  Date Updated : 2026-01-28 | Vex | Support Result and Result<Dictionary<string, object>> handlers
 *                 2026-02-01 | Vex | Added activity name, description, and category to scanner
 *
 *************************************************************/

using System.Reflection;
using System.Text.Json;
using Vexit.Common.Models;
using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Core.Attributes;
using Vexit.FlowEngine.Enums;

namespace Vexit.FlowEngine.Discovery.Scanners;

/// <summary>
/// Scans assemblies for activity methods and registers them in the activity registry.
/// </summary>
public sealed class ActivityScanner
{
    public void Scan(Assembly assembly, IServiceProvider serviceProvider, IActivityRegistry registry)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        if (registry == null) throw new ArgumentNullException(nameof(registry));

        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var attribute = method.GetCustomAttribute<ActivityAttribute>(inherit: false);
                if (attribute == null)
                {
                    continue;
                }

                var key = attribute.Key;
                var activityType = ActivityTypeSE.FromEnum(attribute.Type);
                var activityName = attribute.Name;
                var activityDescription = attribute.Description;
                var activityCategory = attribute.Category;
                var inputType = GetInputType(method);

                if (IsOutputReturnType(method))
                {
                    var handler = BuildOutputHandler(method, type, serviceProvider, inputType);
                    registry.Register(key, handler, inputType, activityType, activityName, activityDescription, activityCategory);
                }
                else
                {
                    var handler = BuildBasicHandler(method, type, serviceProvider, inputType);
                    registry.Register(key, handler, inputType, activityType, activityName, activityDescription, activityCategory);
                }
            }
        }
    }

    private static Type GetInputType(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return typeof(object);
        }

        if (parameters.Length == 1)
        {
            return parameters[0].ParameterType;
        }

        return typeof(Dictionary<string, object>);
    }

    private static bool IsOutputReturnType(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            returnType = returnType.GetGenericArguments()[0];
        }

        return returnType == typeof(Result<Dictionary<string, object>>);
    }

    private static Func<object?, Task<Result<Dictionary<string, object>>>> BuildOutputHandler(
        MethodInfo method,
        Type declaringType,
        IServiceProvider serviceProvider,
        Type inputType)
    {
        return async input =>
        {
            // DEBUG: Check service resolution
            // Console.WriteLine($"[DEBUG ActivityScanner] Attempting to resolve: {declaringType.FullName}");
            var resolved = method.IsStatic ? null : serviceProvider.GetService(declaringType);
            // Console.WriteLine($"[DEBUG ActivityScanner] Resolution result: {resolved != null}");

            // Lazy service resolution: resolve at execution time, not registration time
            var instance = method.IsStatic
                ? null
                : resolved ?? Activator.CreateInstance(declaringType);

            var arguments = BuildArguments(method, input, inputType);
            var result = method.Invoke(instance, arguments);

            if (result is Task<Result<Dictionary<string, object>>> taskResult)
            {
                return await taskResult;
            }

            if (result is Result<Dictionary<string, object>> directResult)
            {
                return directResult;
            }

            return Result<Dictionary<string, object>>.Success(new Dictionary<string, object>());
        };
    }

    private static Func<object?, Task<Result>> BuildBasicHandler(
        MethodInfo method,
        Type declaringType,
        IServiceProvider serviceProvider,
        Type inputType)
    {
        return async input =>
        {
            // DEBUG: Check service resolution
            // Console.WriteLine($"[DEBUG ActivityScanner] Attempting to resolve: {declaringType.FullName}");
            var resolved = method.IsStatic ? null : serviceProvider.GetService(declaringType);
            // Console.WriteLine($"[DEBUG ActivityScanner] Resolution result: {resolved != null}");

            var instance = method.IsStatic
                ? null
                : resolved ?? Activator.CreateInstance(declaringType);

            var arguments = BuildArguments(method, input, inputType);
            var result = method.Invoke(instance, arguments);

            if (result is Task<Result> taskResult)
            {
                return await taskResult;
            }

            if (result is Result directResult)
            {
                return directResult;
            }

            if (result is Task task)
            {
                await task;
                return Result.Success();
            }

            return Result.Success();
        };
    }

    private static object?[] BuildArguments(MethodInfo method, object? input, Type inputType)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return Array.Empty<object?>();
        }

        if (parameters.Length == 1)
        {
            var converted = ConvertInput(input, inputType);
            return new[] { converted };
        }

        var values = ResolveParameterValues(parameters, input);
        return values;
    }

    private static object?[] ResolveParameterValues(ParameterInfo[] parameters, object? input)
    {
        var values = new object?[parameters.Length];
        var inputDict = ToDictionary(input);

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var name = parameter.Name ?? string.Empty;

            if (inputDict.TryGetValue(name, out var value))
            {
                values[i] = ConvertToType(value, parameter.ParameterType);
            }
            else
            {
                values[i] = parameter.HasDefaultValue ? parameter.DefaultValue : null;
            }
        }

        return values;
    }

    private static object? ConvertInput(object? input, Type targetType)
    {
        if (input == null)
        {
            return null;
        }

        if (targetType.IsInstanceOfType(input))
        {
            return input;
        }

        if (input is JsonElement element)
        {
            return JsonSerializer.Deserialize(element.GetRawText(), targetType);
        }

        try
        {
            return Convert.ChangeType(input, targetType);
        }
        catch
        {
            return input;
        }
    }

    private static object? ConvertToType(object? value, Type targetType)
    {
        if (value == null) return null;
        if (targetType.IsInstanceOfType(value)) return value;

        if (value is JsonElement element)
        {
            return JsonSerializer.Deserialize(element.GetRawText(), targetType);
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

    private static Dictionary<string, object?> ToDictionary(object? input)
    {
        if (input == null)
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        if (input is Dictionary<string, object?> typed)
        {
            return new Dictionary<string, object?>(typed, StringComparer.OrdinalIgnoreCase);
        }

        if (input is Dictionary<string, object> raw)
        {
            return raw.ToDictionary(k => k.Key, v => (object?)v.Value, StringComparer.OrdinalIgnoreCase);
        }

        if (input is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText());
            return dict ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var props = input.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            result[prop.Name] = prop.GetValue(input);
        }

        return result;
    }
}

