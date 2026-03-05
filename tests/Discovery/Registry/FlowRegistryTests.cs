using FluentAssertions;
using Vexit.FlowEngine.Core.Attributes;
using Vexit.FlowEngine.Discovery.Registry;
using Xunit;

namespace Vexit.FlowEngine.Tests.Discovery.Registry;

public class FlowRegistryTests
{
    [Fact]
    public void Register_Throws_When_FlowType_Is_Null()
    {
        var sut = new FlowRegistry();

        Action act = () => sut.Register(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("flowType");
    }

    [Fact]
    public void Register_Uses_FlowAttribute_Id_When_Present()
    {
        var sut = new FlowRegistry();

        sut.Register(typeof(AttributedFlow));

        sut.Contains("deploy-flow").Should().BeTrue();
        var metadata = sut.Get("deploy-flow");
        metadata.Should().NotBeNull();
        metadata!.FlowType.Should().Be(typeof(AttributedFlow));
        metadata.Attribute.Should().NotBeNull();
    }

    [Fact]
    public void Register_Uses_Type_Name_When_Attribute_Missing()
    {
        var sut = new FlowRegistry();

        sut.Register(typeof(PlainFlow));

        sut.Contains(nameof(PlainFlow)).Should().BeTrue();
        sut.Get(nameof(PlainFlow))!.FlowType.Should().Be(typeof(PlainFlow));
    }

    [Fact]
    public void Register_Overwrites_Existing_Entry_For_Same_Id()
    {
        var sut = new FlowRegistry();

        sut.Register(typeof(FirstFlowWithSameId));
        sut.Register(typeof(SecondFlowWithSameId));

        var metadata = sut.Get("same-id");
        metadata.Should().NotBeNull();
        metadata!.FlowType.Should().Be(typeof(SecondFlowWithSameId));
    }

    [Fact]
    public void List_Returns_Registered_Flows()
    {
        var sut = new FlowRegistry();
        sut.Register(typeof(AttributedFlow));
        sut.Register(typeof(PlainFlow));

        var list = sut.List();

        list.Should().HaveCount(2);
    }

    [Flow("deploy-flow")]
    private sealed class AttributedFlow { }

    private sealed class PlainFlow { }

    [Flow("same-id")]
    private sealed class FirstFlowWithSameId { }

    [Flow("same-id")]
    private sealed class SecondFlowWithSameId { }
}
