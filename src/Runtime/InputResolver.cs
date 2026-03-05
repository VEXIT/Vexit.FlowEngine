/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Input binding resolver implementation
 *  Date Updated : 2026-01-28	| Vex | Added XML docs for input resolution flow
 *
 *************************************************************/

using System.Reflection;
using System.Text.Json;
using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Runtime;

/// <summary>
/// Resolves input bindings to actual values from flow instance context, previous step outputs, or literals.
/// </summary>
public sealed class InputResolver : IInputResolver
{
    /// <summary>
    /// Resolves a single input binding into a concrete value.
    /// </summary>
    public object? ResolveInput(
        InputBinding binding,
        FlowInstance flowInstance,
        Dictionary<string, object> previousOutputs)
    {
        if (binding == null) throw new ArgumentNullException(nameof(binding));
        if (flowInstance == null) throw new ArgumentNullException(nameof(flowInstance));

        var source = (binding.Source ?? string.Empty).Trim().ToLowerInvariant();

        return source switch
        {
            "context" => ResolveFromContext(binding.Key, flowInstance.Context),
            "literal" => binding.Value ?? binding.Key,
            "step" => ResolveFromOutputs(binding.Key, previousOutputs),
            _ => throw new InvalidOperationException($"Unknown input source: '{binding.Source}'.")
        };
    }

    /// <summary>
    /// Resolves a set of bindings into a dictionary of inputs.
    /// </summary>
    public Dictionary<string, object> ResolveInputs(
        Dictionary<string, InputBinding> inputBindings,
        FlowInstance flowInstance,
        Dictionary<string, object> previousOutputs)
    {
        if (inputBindings == null) throw new ArgumentNullException(nameof(inputBindings));

        var resolved = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, binding) in inputBindings)
        {
            var value = ResolveInput(binding, flowInstance, previousOutputs);
            if (value != null)
            {
                resolved[key] = value;
            }
        }

        return resolved;
    }

    /// <summary>
    /// Resolves values from the flow context using dot-notation paths.
    /// </summary>
    private static object? ResolveFromContext(string key, Dictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (TryGetPathValue(context, key, out var value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Resolves values from previous step outputs.
    /// </summary>
    private static object? ResolveFromOutputs(string key, Dictionary<string, object> outputs)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (outputs.TryGetValue(key, out var direct))
        {
            return direct;
        }

        // Fall back to the last segment (e.g., "step.output.password" -> "password")
        var lastSegment = key.Contains('.') ? key.Split('.').Last() : key;
        return outputs.TryGetValue(lastSegment, out var fallback) ? fallback : null;
    }

    /// <summary>
    /// Walks a dot-path in a mixed object/context graph.
    /// </summary>
    private static bool TryGetPathValue(Dictionary<string, object> context, string path, out object? value)
    {
        value = null;

        if (context.TryGetValue(path, out var direct))
        {
            value = direct;
            return true;
        }

        var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return false;
        }

        object? current = context;
        foreach (var segment in segments)
        {
            if (current == null)
            {
                return false;
            }

            if (current is Dictionary<string, object> dict)
            {
                if (!dict.TryGetValue(segment, out current))
                {
                    return false;
                }

                continue;
            }

            if (current is JsonElement jsonElement)
            {
                current = ExtractJsonElement(jsonElement, segment);
                if (current == null)
                {
                    return false;
                }

                continue;
            }

            var property = current.GetType().GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                return false;
            }

            current = property.GetValue(current);
        }

        value = current;
        return true;
    }

    /// <summary>
    /// Extracts a property from a JSON element.
    /// </summary>
    private static object? ExtractJsonElement(JsonElement element, string segment)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(segment, out var property))
        {
            return ConvertJsonElement(property);
        }

        return null;
    }

    /// <summary>
    /// Converts JSON elements into CLR-friendly values.
    /// </summary>
    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Object => element,
            JsonValueKind.Array => element,
            _ => null
        };
    }
}

