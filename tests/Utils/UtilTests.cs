using FluentAssertions;
using Vexit.FlowEngine.Utils;
using Xunit;

namespace Vexit.FlowEngine.Tests.Utils;

public class UtilTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ResolvePath_Returns_Input_For_Null_Or_Empty(string? path)
    {
        var result = Util.ResolvePath(path!);

        result.Should().Be(path);
    }

    [Fact]
    public void ResolvePath_Expands_Home_Placeholder()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var result = Util.ResolvePath("~/flow-state");

        result.Should().Be(Path.Combine(home, "flow-state"));
    }

    [Fact]
    public void ResolvePath_Returns_Absolute_Path_For_Relative_Input()
    {
        var result = Util.ResolvePath("relative-flow-dir");

        Path.IsPathRooted(result).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetFlowInstanceFileName_Throws_For_Invalid_InstanceId(string? instanceId)
    {
        Action act = () => Util.GetFlowInstanceFileName(instanceId!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("instanceId");
    }

    [Fact]
    public void GetFlowInstanceFileName_Appends_Json_Extension()
    {
        var result = Util.GetFlowInstanceFileName("flow-123");

        result.Should().Be("flow-123.json");
    }
}
