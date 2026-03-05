/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Attribute for flow metadata
 *  Date Updated : 2026-01-29 | Vex | Added StateStoreType, StateStoreLocation, StateStoreBasePath, ServerConnection, StateStoreFileName
 *
 *************************************************************/

using Vexit.FlowEngine.Enums;

namespace Vexit.FlowEngine.Core.Attributes;

/// <summary>
/// Configures storage and runtime behavior for a specific flow. <br/>
/// Applied to flow classes to override global FlowEngine options.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FlowAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of FlowAttribute.
    /// </summary>
    /// <param name="id">The unique flow identifier.</param>
    public FlowAttribute(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    /// <summary>
    /// Unique identifier for this flow type. <br/>
    /// Set via constructor for explicit control, or framework falls back to class name.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Type of state store to use for this flow.
    /// </summary>
    public StateStoreTypeSE? StateStoreType { get; set; }

    /// <summary>
    /// Location where state is stored for this flow.
    /// </summary>
    public StateStoreLocationSE? StateStoreLocation { get; set; }

    /// <summary>
    /// Base path for state storage. Supports ~ for home directory.
    /// </summary>
    public string? StateStoreBasePath { get; set; }

    /// <summary>
    /// Server connection for remote storage (host alias or connection string).
    /// </summary>
    public string? ServerConnection { get; set; }

    /// <summary>
    /// Custom filename for state storage.
    /// If not specified, uses flow ID or GUID-based naming.
    /// </summary>
    public string? StateStoreFileName { get; set; }

    /// <summary>
    /// Optional friendly name for logging/UI.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional description for documentation.
    /// </summary>
    public string? Description { get; set; }
}