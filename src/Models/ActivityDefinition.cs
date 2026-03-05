/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Per-step configuration for activities
 *  Date Updated : 2026-01-28	| Vex | Made Id, Name, and Handler required properties
 *
 *************************************************************/

using System.Text.Json.Serialization;
using Vexit.FlowEngine.Enums;

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Configuration for an individual activity step. <br/>
/// Defines inputs, outputs, transitions, and metadata.
/// </summary>
public class ActivityDefinition
{
    /// <summary>Unique step ID within the flow.</summary>
    public required string Id { get; set; }

    /// <summary>Human-readable name for logging/UI.</summary>
    public required string Name { get; set; }

    /// <summary>Activity type.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActivityTypeEnum Type { get; set; }

    /// <summary>Handler key (e.g., "install.postgres").</summary>
    public required string Handler { get; set; }

    /// <summary>Input bindings (context, literals, etc.).</summary>
    public Dictionary<string, InputBinding> Inputs { get; set; } = new();

    /// <summary>Output mappings back to context.</summary>
    public Dictionary<string, string> Outputs { get; set; } = new();

    /// <summary>List of possible transition rules (when result matches, go to step).</summary>
    public List<Transition> Transitions { get; set; } = new();

    /// <summary>Optional metadata (retries, timeouts).</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
