/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Persisted execution state for workflow instances
 *  Date Updated : 2026-01-28	| Vex | Made FlowDefinitionId required, CurrentStepId nullable
 *
 *************************************************************/

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Persisted execution state for a running workflow instance. <br/>
/// Enables resuming after failures, auditing, and progress tracking.
/// </summary>
public class FlowInstance
{
    /// <summary>Unique instance ID (controlled by consumer).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Reference to the flow definition ID.</summary>
    public required string FlowDefinitionId { get; set; }

    /// <summary>Current phase or step ID for resumability.</summary>
    public string? CurrentStepId { get; set; }

    /// <summary>Shared context data (key-value pairs).</summary>
    public Dictionary<string, object> Context { get; set; } = new();

    /// <summary>History of completed steps.</summary>
    public List<ActivityInstance> CompletedSteps { get; set; } = new();

    /// <summary>Overall status (Running, Completed, Failed).</summary>
    public string Status { get; set; } = "Running";

    /// <summary>When the flow instance started.</summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>When the flow instance completed (null if running).</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Schema version for migration purposes.</summary>
    public string SchemaVersion { get; set; } = "2026.1";
}