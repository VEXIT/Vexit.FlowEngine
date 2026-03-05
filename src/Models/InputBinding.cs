/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Input binding for activity parameters
 *  Date Updated : 2026-01-28	| Vex | Made Source and Key properties required
 *
 *************************************************************/

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Defines how inputs are resolved (context, literal values, step outputs).
/// </summary>
public class InputBinding
{
    /// <summary>Source type: "context", "literal", "step".</summary>
    public required string Source { get; set; }

    /// <summary>
    /// The lookup key or path. <br/>
    /// Supports direct keys (e.g. "IpAddress") or paths (e.g. "Server.Ip").
    /// </summary>
    public required string Key { get; set; }

    /// <summary>Literal value if source is "literal".</summary>
    public object? Value { get; set; }
}