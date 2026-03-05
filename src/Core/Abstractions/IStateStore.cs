/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - State persistence contract
 *  Date Updated :
 *
 *************************************************************/

using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Core.Abstractions;

/// <summary>
/// Abstraction for persisting flow execution state.
/// </summary>
public interface IStateStore
{
    /// <summary>
    /// Saves a flow instance state.
    /// </summary>
    /// <param name="instance">The flow instance to save.</param>
    Task SaveFlowInstanceAsync(FlowInstance instance);

    /// <summary>
    /// Loads a flow instance state by ID.
    /// </summary>
    /// <param name="flowInstanceId">The flow instance ID.</param>
    /// <returns>The flow instance, or null if not found.</returns>
    Task<FlowInstance?> LoadFlowInstanceAsync(string flowInstanceId);

}