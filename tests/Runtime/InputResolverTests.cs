using FluentAssertions;
using System.Text.Json;
using Vexit.FlowEngine.Models;
using Vexit.FlowEngine.Runtime;
using Xunit;

namespace Vexit.FlowEngine.Tests.Runtime;

public class InputResolverTests
{
    private readonly InputResolver _sut = new();

    [Fact]
    public void ResolveInput_Throws_When_Binding_Is_Null()
    {
        var instance = CreateFlowInstance();
        var outputs = new Dictionary<string, object>();

        Action act = () => _sut.ResolveInput(null!, instance, outputs);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("binding");
    }

    [Fact]
    public void ResolveInput_Resolves_Context_Path()
    {
        var instance = CreateFlowInstance();
        instance.Context["Server"] = new Dictionary<string, object>
        {
            ["Ip"] = "10.0.0.10"
        };

        var binding = new InputBinding { Source = "context", Key = "Server.Ip" };

        var result = _sut.ResolveInput(binding, instance, new Dictionary<string, object>());

        result.Should().Be("10.0.0.10");
    }

    [Fact]
    public void ResolveInput_Resolves_Context_From_Object_Property()
    {
        var instance = CreateFlowInstance();
        instance.Context["Server"] = new ServerInfo { Host = "prod-01" };
        var binding = new InputBinding { Source = "context", Key = "Server.Host" };

        var result = _sut.ResolveInput(binding, instance, new Dictionary<string, object>());

        result.Should().Be("prod-01");
    }

    [Fact]
    public void ResolveInput_Resolves_Context_From_JsonElement()
    {
        var instance = CreateFlowInstance();
        var json = JsonDocument.Parse("{\"Database\":{\"Port\":5432}}").RootElement.Clone();
        instance.Context["Config"] = json;
        var binding = new InputBinding { Source = "context", Key = "Config.Database.Port" };

        var result = _sut.ResolveInput(binding, instance, new Dictionary<string, object>());

        result.Should().Be(5432L);
    }

    [Fact]
    public void ResolveInput_Resolves_Literal_Value()
    {
        var binding = new InputBinding { Source = "literal", Key = "fallback", Value = "explicit-value" };

        var result = _sut.ResolveInput(binding, CreateFlowInstance(), new Dictionary<string, object>());

        result.Should().Be("explicit-value");
    }

    [Fact]
    public void ResolveInput_Uses_Key_When_Literal_Value_Is_Null()
    {
        var binding = new InputBinding { Source = "literal", Key = "fallback-key", Value = null };

        var result = _sut.ResolveInput(binding, CreateFlowInstance(), new Dictionary<string, object>());

        result.Should().Be("fallback-key");
    }

    [Fact]
    public void ResolveInput_Resolves_Step_Output_By_Last_Segment_Fallback()
    {
        var outputs = new Dictionary<string, object>
        {
            ["password"] = "secret"
        };
        var binding = new InputBinding { Source = "step", Key = "setup.output.password" };

        var result = _sut.ResolveInput(binding, CreateFlowInstance(), outputs);

        result.Should().Be("secret");
    }

    [Fact]
    public void ResolveInput_Throws_For_Unknown_Source()
    {
        var binding = new InputBinding { Source = "unknown", Key = "x" };

        Action act = () => _sut.ResolveInput(binding, CreateFlowInstance(), new Dictionary<string, object>());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unknown input source*");
    }

    [Fact]
    public void ResolveInputs_Skips_Unresolved_Null_Values()
    {
        var bindings = new Dictionary<string, InputBinding>
        {
            ["resolved"] = new() { Source = "literal", Key = "ignored", Value = "ok" },
            ["missing"] = new() { Source = "context", Key = "Nope.Path" }
        };

        var result = _sut.ResolveInputs(bindings, CreateFlowInstance(), new Dictionary<string, object>());

        result.Should().ContainKey("resolved");
        result.Should().NotContainKey("missing");
    }

    private static FlowInstance CreateFlowInstance()
        => new()
        {
            Id = "flow-instance-1",
            FlowDefinitionId = "flow-def",
            Context = new Dictionary<string, object>()
        };

    private sealed class ServerInfo
    {
        public string Host { get; set; } = string.Empty;
    }
}
