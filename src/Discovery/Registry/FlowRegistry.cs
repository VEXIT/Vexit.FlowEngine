/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Flow registry implementation
 *  Date Updated : 
 *
 *************************************************************/

using Vexit.FlowEngine.Core.Attributes;

namespace Vexit.FlowEngine.Discovery.Registry;

/// <summary>
/// Registry that maps flow IDs to their metadata and types.
/// </summary>
public sealed class FlowRegistry
{
    public sealed record FlowMetadata(string Id, Type FlowType, FlowAttribute? Attribute);

    private readonly Dictionary<string, FlowMetadata> _flows =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(Type flowType)
    {
        if (flowType == null) throw new ArgumentNullException(nameof(flowType));

        var attribute = flowType.GetCustomAttributes(typeof(FlowAttribute), inherit: false)
            .Cast<FlowAttribute>()
            .FirstOrDefault();

        var id = attribute?.Id ?? flowType.Name;
        _flows[id] = new FlowMetadata(id, flowType, attribute);
    }

    public FlowMetadata? Get(string id)
        => _flows.TryGetValue(id, out var metadata) ? metadata : null;

    public bool Contains(string id)
        => _flows.ContainsKey(id);

    public IReadOnlyCollection<FlowMetadata> List()
        => _flows.Values.ToList();
}

