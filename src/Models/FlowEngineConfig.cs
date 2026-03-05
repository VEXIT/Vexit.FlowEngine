/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - Flow engine hosting options
 *  Date Updated : 2026-01-29 | Vex | Added StateStoreType, StateStoreLocation, StateStoreBasePath, DefaultServerConnection
 *
 *************************************************************/

using System.Reflection;
using Vexit.FlowEngine.Enums;

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Options for configuring FlowEngine hosting and defaults.
/// </summary>
public sealed class FlowEngineConfig
{
    /// <summary>
    /// Type of state store to use.
    /// </summary>
    public StateStoreTypeSE StateStoreType { get; set; } = StateStoreTypeSE.File;

    /// <summary>
    /// Location where state is stored.
    /// </summary>
    public StateStoreLocationSE StateStoreLocation { get; set; } = StateStoreLocationSE.Local;

    /// <summary>
    /// Base path for state storage. Supports ~ for home directory.
    /// </summary>
    public string StateStoreBasePath { get; set; } = "~/.vexit/flow-state";

    /// <summary>
    /// Default server connection for remote storage (host alias or connection string).
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Assembly used for discovery by default.
    /// </summary>
    public Assembly? DiscoveryAssembly { get; set; } = Assembly.GetEntryAssembly();
}

