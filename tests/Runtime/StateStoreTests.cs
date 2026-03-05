using FluentAssertions;
using Vexit.FlowEngine.Models;
using Vexit.FlowEngine.Runtime;
using Xunit;

namespace Vexit.FlowEngine.Tests.Runtime;

public class StateStoreTests
{
    [Fact]
    public async Task SaveFlowInstanceAsync_Throws_When_Instance_Is_Null()
    {
        var sut = CreateStateStore(Path.Combine(Path.GetTempPath(), "flow-store-tests", Guid.NewGuid().ToString("N")));

        var act = async () => await sut.SaveFlowInstanceAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("instance");
    }

    [Fact]
    public async Task LoadFlowInstanceAsync_Returns_Null_When_File_Does_Not_Exist()
    {
        var basePath = Path.Combine(Path.GetTempPath(), "flow-store-tests", Guid.NewGuid().ToString("N"));
        var sut = CreateStateStore(basePath);

        var result = await sut.LoadFlowInstanceAsync("missing-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveFlowInstanceAsync_Then_LoadFlowInstanceAsync_RoundTrips_Data()
    {
        var basePath = Path.Combine(Path.GetTempPath(), "flow-store-tests", Guid.NewGuid().ToString("N"));
        var sut = CreateStateStore(basePath);
        var instance = new FlowInstance
        {
            Id = "instance-1",
            FlowDefinitionId = "deploy-flow",
            Status = "Running",
            Context = new Dictionary<string, object>
            {
                ["env"] = "prod"
            }
        };

        await sut.SaveFlowInstanceAsync(instance);
        var loaded = await sut.LoadFlowInstanceAsync("instance-1");

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be("instance-1");
        loaded.FlowDefinitionId.Should().Be("deploy-flow");
        loaded.Status.Should().Be("Running");
        loaded.Context.Should().ContainKey("env");

        try { Directory.Delete(basePath, true); } catch { }
    }

    [Fact]
    public async Task LoadFlowInstanceAsync_Throws_When_InstanceId_Contains_Invalid_Chars()
    {
        var sut = CreateStateStore(Path.Combine(Path.GetTempPath(), "flow-store-tests", Guid.NewGuid().ToString("N")));

        var act = async () => await sut.LoadFlowInstanceAsync("bad/name");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("instanceId");
    }

    private static StateStore CreateStateStore(string basePath)
    {
        var config = new FlowEngineConfig
        {
            StateStoreBasePath = basePath
        };
        return new StateStore(config);
    }
}
