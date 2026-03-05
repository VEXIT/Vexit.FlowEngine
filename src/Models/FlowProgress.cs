/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-02-01 - Flow progress information for observers
 *  Date Updated :
 *
 *************************************************************/

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Represents the current progress of a flow execution.
/// </summary>
public class FlowProgress
{
    /// <summary>
    /// Number of steps that have been completed.
    /// </summary>
    public int CompletedSteps { get; set; }

    /// <summary>
    /// Total number of steps in the flow.
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// Calculated percentage of completion (0-100).
    /// </summary>
    public int PercentComplete => TotalSteps > 0
        ? (int)Math.Round(((double)CompletedSteps / TotalSteps) * 100)
        : 0;
}