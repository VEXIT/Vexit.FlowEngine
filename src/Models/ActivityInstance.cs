/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Execution record for activity runs
 *  Date Updated : 2026-01-28 | Vex | Made ActivityDefinitionId required property
 *                 2026-02-01 | Vex | Changed Status from string to ActivityStatusEnum
 *
 *************************************************************/

using System.Text.Json.Serialization;
using Vexit.FlowEngine.Enums;

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Execution record for a single activity run. <br/>
/// Tracks timing, results, and retry history.
/// </summary>
public class ActivityInstance
{
    /// <summary>Reference to the activity definition ID.</summary>
    public required string ActivityDefinitionId { get; set; }

    /// <summary>Status of this execution.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActivityStatusEnum Status { get; set; } = ActivityStatusEnum.Running;

    /// <summary>Message related to the status.</summary>
    public string? StatusMessage { get; set; }

    /// <summary>When execution started.</summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>When execution completed (null if running).</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Number of retry attempts.</summary>
    public int AttemptCount { get; set; } = 1;

    /// <summary>Input values used for this execution.</summary>
    public Dictionary<string, object> Inputs { get; set; } = new();

    /// <summary>Output/result values from execution.</summary>
    public Dictionary<string, object> Outputs { get; set; } = new();

}