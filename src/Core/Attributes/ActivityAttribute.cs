/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Attribute for marking activity methods
 *  Date Updated :
 *
 *************************************************************/

using Vexit.FlowEngine.Enums;

namespace Vexit.FlowEngine.Core.Attributes;

/// <summary>
/// Marks a method as a flow activity that can be discovered and registered. <br/>
/// Activities are the executable units of work in flows.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ActivityAttribute : Attribute
{
    /// <summary>
    /// The unique key for this activity (e.g., "ssh.connect").
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The activity type (automatically set by derived attributes).
    /// </summary>
    public ActivityTypeEnum Type { get; protected set; }

    /// <summary>
    /// Optional friendly name for logging/UI.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional description for documentation and AI tool manifests.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional category for grouping activities.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Initializes a new instance of ActivityAttribute.
    /// </summary>
    /// <param name="key">The unique activity key.</param>
    public ActivityAttribute(string key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Type = ActivityTypeEnum.Step; // Default to Step
    }
}

/// <summary>
/// Marks a method as a Step activity (executable unit of work).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class StepAttribute : ActivityAttribute
{
    public StepAttribute(string key) : base(key)
    {
        Type = ActivityTypeEnum.Step;
    }
}

/// <summary>
/// Marks a method as a Group activity (container for sequential/parallel activities).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class GroupAttribute : ActivityAttribute
{
    public GroupAttribute(string key) : base(key)
    {
        Type = ActivityTypeEnum.Group;
    }
}

/// <summary>
/// Marks a method as a Decision activity (branching logic based on conditions).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DecisionAttribute : ActivityAttribute
{
    public DecisionAttribute(string key) : base(key)
    {
        Type = ActivityTypeEnum.Decision;
    }
}