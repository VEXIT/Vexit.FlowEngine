/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Flow scanner for reflection discovery
 *  Date Updated :
 *
 *************************************************************/

using System.Reflection;
using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Discovery.Registry;

namespace Vexit.FlowEngine.Discovery.Scanners;

/// <summary>
/// Scans assemblies for flow types and registers them in the flow registry.
/// </summary>
public sealed class FlowScanner
{
    public void Scan(Assembly assembly, FlowRegistry registry)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        if (registry == null) throw new ArgumentNullException(nameof(registry));

        foreach (var type in assembly.GetTypes())
        {
            if (IsFlowType(type))
            {
                registry.Register(type);
            }
        }
    }

    private static bool IsFlowType(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            return false;
        }

        return type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IFlow<>));
    }
}

