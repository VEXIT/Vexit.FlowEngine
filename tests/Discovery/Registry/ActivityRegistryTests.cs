using FluentAssertions;
using Vexit.Common.Models;
using Vexit.FlowEngine.Discovery.Registry;
using Vexit.FlowEngine.Enums;
using Xunit;

namespace Vexit.FlowEngine.Tests.Discovery.Registry;

public class ActivityRegistryTests
{
    [Fact]
    public void Register_Basic_Throws_When_Key_Is_Empty()
    {
        var sut = new ActivityRegistry();
        Func<object?, Task<Result>> handler = _ => Task.FromResult(Result.Ok());

        Action act = () => sut.Register("", handler, typeof(object), ActivityTypeSE.Step, null, null, null);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task Register_Basic_Stores_Handler_And_Metadata()
    {
        var sut = new ActivityRegistry();
        Func<object?, Task<Result>> handler = _ => Task.FromResult(Result.Ok("done"));

        sut.Register("deploy.run", handler, typeof(Dictionary<string, object>), ActivityTypeSE.Step, "Deploy", "Run deploy", "Deployment");

        sut.Contains("deploy.run").Should().BeTrue();
        sut.GetBasic("deploy.run").Should().NotBeNull();
        sut.GetInputType("deploy.run").Should().Be(typeof(Dictionary<string, object>));
        sut.GetActivityType("deploy.run")!.EnumValue.Should().Be(ActivityTypeEnum.Step);
        sut.GetActivityName("deploy.run").Should().Be("Deploy");
        sut.GetActivityDescription("deploy.run").Should().Be("Run deploy");
        sut.GetActivityCategory("deploy.run").Should().Be("Deployment");

        var info = sut.GetActivityInfo("deploy.run");
        info.Should().NotBeNull();
        info!.Key.Should().Be("deploy.run");
        info.Name.Should().Be("Deploy");
        info.Category.Should().Be("Deployment");

        var result = await sut.GetBasic("deploy.run")!(null);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithOutputs_Stores_Output_Handler()
    {
        var sut = new ActivityRegistry();
        Func<object?, Task<Result<Dictionary<string, object>>>> handler =
            _ => Task.FromResult(Result<Dictionary<string, object>>.Success(new Dictionary<string, object> { ["token"] = "abc" }));

        sut.Register("auth.issue", handler, typeof(object), ActivityTypeSE.Step, "Issue Token", null, "Auth");

        sut.GetWithOutputs("auth.issue").Should().NotBeNull();
        var result = await sut.GetWithOutputs("auth.issue")!(null);
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainKey("token");
    }

    [Fact]
    public void GetActivityInfo_Returns_Null_For_Unknown_Key()
    {
        var sut = new ActivityRegistry();

        var result = sut.GetActivityInfo("unknown");

        result.Should().BeNull();
    }
}
