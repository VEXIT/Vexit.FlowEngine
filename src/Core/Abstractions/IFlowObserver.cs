/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Flow observer interface for execution telemetry
 *  Date Updated : 2026-01-31 | Vex | Add activity-based observer callbacks
 *                 2026-02-01 | Vex | Use ActivityInfo and ActivityState DTOs in observer callbacks
 *
 *************************************************************/

using Vexit.Common.Models;
using Vexit.FlowEngine.Enums;
using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Core.Abstractions;

/// <summary>
/// Optional observer for flow execution events.
/// Implement in consumer applications to handle logging, UI output, or telemetry.
/// </summary>
public interface IFlowObserver
{
    /// <summary>
    /// Called when a flow starts.
    /// </summary>
    void OnFlowStarted(string flowId, string instanceId, int totalSteps);

    /// <summary>
    /// Called when a flow completes successfully.
    /// </summary>
    void OnFlowCompleted(string flowId, string instanceId, TimeSpan duration);

    /// <summary>
    /// Called when a flow fails.
    /// </summary>
    void OnFlowFailed(string flowId, string instanceId, Exception exception);

    /// <summary>
    /// Called when an activity starts.
    /// </summary>
    void OnActivityStarted(ActivityInfo activityInfo, ActivityState activityState, FlowProgress progress);

    /// <summary>
    /// Called when an activity completes.
    /// </summary>
    void OnActivityCompleted(ActivityInfo activityInfo, Result<ActivityState> result, FlowProgress progress);

    /// <summary>
    /// Called when an activity is skipped because it was already completed.
    /// </summary>
    void OnActivitySkipped(ActivityInfo activityInfo, ActivityState activityState, FlowProgress progress);
}
