using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Models;
using Xunit;

namespace Vexit.FlowEngine.Tests;

public class FlowEngineExtensionsTests
{
    [Fact]
    public void AddVexitFlowEngine_Registers_Core_Services()
    {
        var services = new ServiceCollection();
        services.AddVexitFlowEngine();
        var provider = services.BuildServiceProvider();

        var config = provider.GetService<FlowEngineConfig>();
        using var scope = provider.CreateScope();
        var flowRunner = scope.ServiceProvider.GetService<IFlowRunner>();
        var flowController = scope.ServiceProvider.GetService<IFlowController>();
        var stateStore = provider.GetService<IStateStore>();

        config.Should().NotBeNull();
        flowRunner.Should().NotBeNull();
        flowController.Should().NotBeNull();
        stateStore.Should().NotBeNull();
    }

    [Fact]
    public void AddVexitFlowEngine_Applies_Configuration_Action()
    {
        var services = new ServiceCollection();
        services.AddVexitFlowEngine(options =>
        {
            options.StateStoreBasePath = "~/custom-flow-state";
        });
        var provider = services.BuildServiceProvider();

        var config = provider.GetRequiredService<FlowEngineConfig>();

        config.StateStoreBasePath.Should().Be("~/custom-flow-state");
    }

    [Fact]
    public void AddVexitFlowEngine_On_HostBuilder_Registers_Configuration()
    {
        var builder = Host.CreateApplicationBuilder([]);
        builder.AddVexitFlowEngine(options => options.StateStoreBasePath = "~/host-flow-state");
        var provider = builder.Services.BuildServiceProvider();

        var config = provider.GetRequiredService<FlowEngineConfig>();

        config.StateStoreBasePath.Should().Be("~/host-flow-state");
    }
}
