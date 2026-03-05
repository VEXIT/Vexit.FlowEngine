/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Attribute for input parameter binding
 *  Date Updated :
 *
 *************************************************************/

namespace Vexit.FlowEngine.Core.Attributes;

/// <summary>
/// Defines how an activity method parameter should be bound. <br/>
/// Provides metadata for input resolution and AI tool manifests.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class InputAttribute : Attribute
{
    /// <summary>
    /// The parameter name/key for binding.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Optional description for documentation and AI tool manifests.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional default value if parameter is not provided.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Whether this parameter is required.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of InputAttribute.
    /// </summary>
    /// <param name="name">The parameter name for binding.</param>
    public InputAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}