/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-02-01 - Activity execution state for observers
 *  Date Updated : 2026-02-02 | Vex | Add FromActivityInstance method
 *
 *************************************************************/

using System.Text.Json.Serialization;
using Vexit.FlowEngine.Enums;

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Execution state of an activity for observer notifications. <br/>
/// Contains runtime execution data without heavy payload details.
/// </summary>
public class ActivityState
{
    /// <summary>
    /// Current execution status.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ActivityStatusEnum Status { get; set; } = ActivityStatusEnum.Pending;

    /// <summary>
    /// When execution started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// When execution completed (null if running).
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Number of retry attempts.
    /// </summary>
    public int AttemptCount { get; set; } = 1;

    /// <summary>
    /// Computed duration of execution.
    /// </summary>
    public TimeSpan? Duration => CompletedAt.HasValue
        ? CompletedAt - StartedAt
        : null;

    /// <summary>
    /// Creates an ActivityState from an ActivityInstance.
    /// </summary>
    /// <param name="activityInstance">The activity instance to map from.</param>
    /// <returns>A new ActivityState with mapped values.</returns>
    public static ActivityState FromActivityInstance(ActivityInstance activityInstance)
    {
        return new ActivityState
        {
            Status = activityInstance.Status,
            StartedAt = activityInstance.StartedAt,
            CompletedAt = activityInstance.CompletedAt,
            AttemptCount = activityInstance.AttemptCount
        };
    }
}