/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Flow orchestration contract
 *  Date Updated :
 *
 *************************************************************/

namespace Vexit.FlowEngine.Core.Abstractions;

/// <summary>
/// Defines the orchestration logic for a workflow. Implement this to describe what your flow does.
/// </summary>
/// <typeparam name="TContext">The context type containing flow data.</typeparam>
public interface IFlow<TContext> where TContext : class
{
    /// <summary>
    /// Builds the flow logic using the provided controller.
    /// </summary>
    /// <param name="controller">The flow controller for executing activities and groups.</param>
    /// <param name="context">The runtime context with input data.</param>
    Task Build(IFlowController controller, TContext context);
}