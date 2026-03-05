/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Activity registry contract
 *  Date Updated : 2026-01-28 | Vex | Support Result and Result<Dictionary<string, object>> handlers
 *                 2026-02-01 | Vex | Added GetActivityInfo method to registry
 *                
 *
 *************************************************************/

using Vexit.Common.Models;
using Vexit.FlowEngine.Enums;
using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Core.Abstractions;

/// <summary>
/// Registry for mapping activity keys to their implementations.
/// </summary>
public interface IActivityRegistry
{
    /// <summary>
    /// Registers an activity implementation that returns a Result without outputs.
    /// </summary>
    /// <param name="key">The activity key (e.g., "ssh.connect").</param>
    /// <param name="activity">The activity implementation delegate.</param>
    /// <param name="inputType">The expected input type for validation.</param>
    /// <param name="activityType">The activity type for progress and observation.</param>
    void Register(string key, Func<object?, Task<Result>> activity, Type inputType, ActivityTypeSE activityType, string? activityName, string? activityDescription, string? activityCategory);

    /// <summary>
    /// Registers an activity implementation that returns outputs.
    /// </summary>
    /// <param name="key">The activity key (e.g., "ssh.connect").</param>
    /// <param name="activity">The activity implementation delegate.</param>
    /// <param name="inputType">The expected input type for validation.</param>
    /// <param name="activityType">The activity type for progress and observation.</param>
    /// <param name="activityName">Human-readable name for logging/UI.</param>
    void Register(string key, Func<object?, Task<Result<Dictionary<string, object>>>> activity, Type inputType, ActivityTypeSE activityType, string? activityName, string? activityDescription, string? activityCategory);

    /// <summary>
    /// Gets an activity implementation by key (no outputs).
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>The activity implementation, or null if not found.</returns>
    Func<object?, Task<Result>>? GetBasic(string key);

    /// <summary>
    /// Gets an activity implementation by key (with outputs).
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>The activity implementation, or null if not found.</returns>
    Func<object?, Task<Result<Dictionary<string, object>>>>? GetWithOutputs(string key);

    /// <summary>
    /// Gets the expected input type for an activity.
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>The expected input type, or null if not registered.</returns>
    Type? GetInputType(string key);

    /// <summary>
    /// Gets the activity type for a registered activity.
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>The activity type, or null if not registered.</returns>
    ActivityTypeSE? GetActivityType(string key);

    /// <summary>
    /// Gets the activity name for a registered activity.
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>The activity name, or null if not registered.</returns>
    string? GetActivityName(string key);

    /// <summary>
    /// Gets the activity description for a registered activity.
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>The activity description, or null if not registered.</returns>
    string? GetActivityDescription(string key);

    /// <summary>
    /// Gets the activity category for a registered activity.
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>The activity category, or null if not registered.</returns>
    string? GetActivityCategory(string key);

    /// <summary>
    /// Gets complete activity information for a registered activity.
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>The activity info, or null if not registered.</returns>
    ActivityInfo? GetActivityInfo(string key);

    /// <summary>
    /// Checks if an activity key is registered.
    /// </summary>
    /// <param name="key">The activity key.</param>
    /// <returns>True if registered, false otherwise.</returns>
    bool Contains(string key);
}