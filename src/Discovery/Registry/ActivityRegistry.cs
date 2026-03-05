/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Activity registry implementation
 *  Date Updated : 2026-01-28 | Vex | Support Result and Result<Dictionary<string, object>> handlers
 *                 2026-02-01 | Vex | Added GetActivityInfo method to registry
 *************************************************************/

using Vexit.Common.Models;
using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Enums;
using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Discovery.Registry;

/// <summary>
/// Registry that maps activity keys to their handlers and input types.
/// </summary>
public sealed class ActivityRegistry : IActivityRegistry
{
    private sealed record ActivityEntry(
        Func<object?, Task<Result>>? BasicHandler,
        Func<object?, Task<Result<Dictionary<string, object>>>>? OutputHandler,
        Type InputType,
        ActivityTypeSE ActivityType,
        string? ActivityName,
        string? ActivityDescription,
        string? ActivityCategory);

    private readonly Dictionary<string, ActivityEntry> _activities =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(string key, Func<object?, Task<Result>> activity, Type inputType, ActivityTypeSE activityType, string? activityName, string? activityDescription, string? activityCategory)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));
        if (activity == null) throw new ArgumentNullException(nameof(activity));
        if (inputType == null) throw new ArgumentNullException(nameof(inputType));

        _activities[key] = new ActivityEntry(activity, GetWithOutputs(key), inputType, activityType, activityName, activityDescription, activityCategory);
    }

    public void Register(string key, Func<object?, Task<Result<Dictionary<string, object>>>> activity, Type inputType, ActivityTypeSE activityType, string? activityName, string? activityDescription, string? activityCategory)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));
        if (activity == null) throw new ArgumentNullException(nameof(activity));
        if (inputType == null) throw new ArgumentNullException(nameof(inputType));

        _activities[key] = new ActivityEntry(GetBasic(key), activity, inputType, activityType, activityName, activityDescription, activityCategory);
    }

    public Func<object?, Task<Result>>? GetBasic(string key)
        => _activities.TryGetValue(key, out var entry) ? entry.BasicHandler : null;

    public Func<object?, Task<Result<Dictionary<string, object>>>>? GetWithOutputs(string key)
        => _activities.TryGetValue(key, out var entry) ? entry.OutputHandler : null;

    public Type? GetInputType(string key)
        => _activities.TryGetValue(key, out var entry) ? entry.InputType : null;

    public ActivityTypeSE? GetActivityType(string key)
        => _activities.TryGetValue(key, out var entry) ? entry.ActivityType : null;

    public string? GetActivityName(string key)
        => _activities.TryGetValue(key, out var entry) ? entry.ActivityName : null;

    public string? GetActivityDescription(string key)
        => _activities.TryGetValue(key, out var entry) ? entry.ActivityDescription : null;

    public string? GetActivityCategory(string key)
        => _activities.TryGetValue(key, out var entry) ? entry.ActivityCategory : null;

    public ActivityInfo? GetActivityInfo(string key)
    {
        if (!_activities.TryGetValue(key, out var entry))
        {
            return null;
        }

        return new ActivityInfo
        {
            Key = key,
            Name = entry.ActivityName ?? string.Empty,
            Type = entry.ActivityType ?? ActivityTypeSE.Step,
            Description = entry.ActivityDescription,
            Category = entry.ActivityCategory
        };
    }

    public bool Contains(string key)
        => _activities.ContainsKey(key);
}

