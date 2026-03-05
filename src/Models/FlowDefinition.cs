/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-15 - Flow definition model for workflow blueprints
 *  Date Updated : 2026-01-17	| Vex | Renamed from Flow.cs to FlowDefinition.cs for clarity
 *               : 2026-01-28	| Vex | Made Id, Name, and StartStepId required properties
 *
 *************************************************************/

namespace Vexit.FlowEngine.Models;

/// <summary>
/// JSON blueprint for a workflow process (steps/transitions). <br/>
/// This is the static "recipe" loaded from JSON files.
/// </summary>
public class FlowDefinition
{
    /// <summary>Unique identifier for the flow definition.</summary>
    public required string Id { get; set; }

    /// <summary>The display name of the flow (e.g., "Server Setup").</summary>
    public required string Name { get; set; }

    /// <summary>The version of this flow definition for schema migration.</summary>
    public string Version { get; set; } = "2026.1";

    /// <summary>
    /// The starting step ID. Due to the graph design, <br/>
    /// execution follows transitions between steps.
    /// </summary>
    public required string StartStepId { get; set; }

    /// <summary>Map of step IDs to their definitions.</summary>
    public Dictionary<string, ActivityDefinition> Steps { get; set; } = new();
}