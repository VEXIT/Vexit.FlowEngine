/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-28 - FlowEngine extension methods
 *  Date Updated : 2026-01-28	| Vex | Recreated with proper smart enum usage
 *
 *************************************************************/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Discovery.Registry;
using Vexit.FlowEngine.Discovery.Scanners;
using Vexit.FlowEngine.Utils;
using Vexit.FlowEngine.Models;
using Vexit.FlowEngine.Runtime;

namespace Vexit.FlowEngine;

/// <summary>
/// Extension methods for configuring FlowEngine services.
/// </summary>
public static class FlowEngineExtensions
{
    /// <summary>
    /// Adds Vexit FlowEngine services to a service collection (for CLI registries).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action for FlowEngine options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVexitFlowEngine(
        this IServiceCollection services,
        Action<FlowEngineConfig>? configure = null)
    {
        var options = new FlowEngineConfig();
        configure?.Invoke(options);

        RegisterFlowEngineServices(services, options);
        return services;
    }

    /// <summary>
    /// Adds Vexit FlowEngine services to the host application builder.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configure">Optional configuration action for FlowEngine options.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddVexitFlowEngine(
        this IHostApplicationBuilder builder,
        Action<FlowEngineConfig>? configure = null)
    {
        var options = new FlowEngineConfig();
        configure?.Invoke(options);

        RegisterFlowEngineServices(builder.Services, options);
        return builder;
    }

    private static void RegisterFlowEngineServices(IServiceCollection services, FlowEngineConfig options)
    {
        // Register options
        services.AddSingleton(options);

        // Register core services
        services.AddSingleton<IActivityRegistry, ActivityRegistry>();
        services.AddSingleton<FlowRegistry>();
        services.AddSingleton<ActivityScanner>();
        services.AddSingleton<FlowScanner>();

        services.AddScoped<IInputResolver, InputResolver>();

        // Register state store based on configuration
        services.AddSingleton<IStateStore, StateStore>();

        services.AddScoped<IFlowRunner, FlowOrchestrator>();
        services.AddScoped<IFlowController, FlowOrchestrator>();
    }

}