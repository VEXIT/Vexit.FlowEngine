/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Main entry point for running flows
 *  Date Updated : 2026-01-28 | Vex | Added class constraint to TFlow generic parameters
 *                 2026-02-06 | Vex | Added storageConfig parameter to RunAsync method
 *************************************************************/

using Vexit.Common.Models;
using Vexit.FlowEngine.Models;

namespace Vexit.FlowEngine.Core.Abstractions;

/// <summary>
/// Main entry point for executing flows. Injected into CLI commands and services.
/// </summary>
public interface IFlowRunner
{
    /// <summary>
    /// Runs a new flow instance with the provided context.
    /// </summary>
    /// <typeparam name="TFlow">The flow type to execute.</typeparam>
    /// <typeparam name="TContext">The context type for the flow.</typeparam>
    /// <param name="instanceId">The ID of the flow instance to run.</param>
    /// <param name="context">The runtime context containing input data.</param>
    /// <returns>Result indicating success or failure of the flow execution.</returns>
    Task<Result> RunAsync<TFlow, TContext>(string instanceId, TContext context)
        where TFlow : class, IFlow<TContext>
        where TContext : class;

    /// <summary>
    /// Resumes an existing flow instance by ID.
    /// </summary>
    /// <typeparam name="TFlow">The flow type to resume.</typeparam>
    /// <typeparam name="TContext">The context type for the flow.</typeparam>
    /// <param name="instanceId">The ID of the flow instance to resume.</param>
    /// <returns>Result indicating success or failure of the resumed flow execution.</returns>
    Task<Result> ResumeAsync<TFlow, TContext>(string instanceId)
        where TFlow : class, IFlow<TContext>
        where TContext : class;
}