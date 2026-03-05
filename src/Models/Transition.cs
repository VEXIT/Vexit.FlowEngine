/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Transition rules between activity steps
 *  Date Updated : 2026-01-28	| Vex | Made When and To properties required
 *
 *************************************************************/

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Defines a single transition rule for an activity step.
/// </summary>
public class Transition
{
    /// <summary>The value to match against the activity result (e.g., "Success", "Failed", "Retry").</summary>
    public required string When { get; set; }

    /// <summary>The ID of the activity to move to.</summary>
    public required string To { get; set; }

    /// <summary>Optional: A C# expression or JSONPath condition for complex branching.</summary>
    public string? Condition { get; set; }
}