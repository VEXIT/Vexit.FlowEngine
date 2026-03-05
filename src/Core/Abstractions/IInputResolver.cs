 /*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Input resolution contract
 *  Date Updated :
 *
 *************************************************************/

using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Core.Abstractions;

/// <summary>
/// Resolves input bindings to actual values from flow instance context, previous steps, or literals.
/// </summary>
public interface IInputResolver
{
    /// <summary>
    /// Resolves a single input binding to its value.
    /// </summary>
    /// <param name="binding">The input binding configuration.</param>
    /// <param name="flowInstance">The current flow instance with context data.</param>
    /// <param name="previousOutputs">Outputs from previous steps.</param>
    /// <returns>The resolved value.</returns>
    object? ResolveInput(InputBinding binding, FlowInstance flowInstance, Dictionary<string, object> previousOutputs);

    /// <summary>
    /// Resolves all input bindings for an activity.
    /// </summary>
    /// <param name="inputBindings">The input bindings to resolve.</param>
    /// <param name="flowInstance">The current flow instance with context data.</param>
    /// <param name="previousOutputs">Outputs from previous steps.</param>
    /// <returns>Dictionary of resolved input values.</returns>
    Dictionary<string, object> ResolveInputs(Dictionary<string, InputBinding> inputBindings, FlowInstance flowInstance, Dictionary<string, object> previousOutputs);
}