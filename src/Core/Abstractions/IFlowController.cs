/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Flow control surface for orchestration
 *  Date Updated : 2026-01-29 | Vex | Added flow storage configuration
 *
 *************************************************************/

using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Core.Abstractions;

/// <summary>
/// Control surface for flow orchestration. Used within IFlow implementations to execute activities and groups.
/// </summary>
public interface IFlowController
{
    /// <summary>
    /// Access to the current flow instance for conditional logic and resume support.
    /// </summary>
    FlowInstance Instance { get; }

    /// <summary>
    /// Executes a Step activity (executable unit of work).
    /// </summary>
    /// <param name="activityKey">The registered activity key (e.g., "server-setup.connect-initial").</param>
    /// <param name="inputs">Input parameters for the activity.</param>
    Task Step(string activityKey, object? inputs = null);

    /// <summary>
    /// Executes activities in a sequential group.
    /// </summary>
    /// <param name="groupName">Name for logging/tracking.</param>
    /// <param name="actions">The actions to execute in sequence.</param>
    Task Group(string groupName, Func<Task> actions);

    /// <summary>
    /// Executes activities in a parallel group.
    /// </summary>
    /// <param name="groupName">Name for logging/tracking.</param>
    /// <param name="actions">The actions to execute in parallel.</param>
    Task GroupParallel(string groupName, Func<Task> actions);

    /// <summary>
    /// Executes conditional branching logic.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="then">Actions to execute if condition is true.</param>
    /// <param name="else">Actions to execute if condition is false.</param>
    Task Decide(Func<bool> condition, Func<Task> then, Func<Task>? @else = null);
}