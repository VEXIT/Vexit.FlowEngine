/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-29 - State store location smart enum
 *  Date Updated :
 *
 *************************************************************/

using Vexit.Common.BaseClasses;

namespace Vexit.FlowEngine.Enums;

/// <summary>
/// Smart enum wrapper for <see cref="StateStoreLocationEnum"/> providing string helpers.
/// Instances are auto-generated from enum values.
/// </summary>
public sealed class StateStoreLocationSE : SmartEnumBase<StateStoreLocationEnum, StateStoreLocationSE>
{
    public static StateStoreLocationSE Local => FromEnum(StateStoreLocationEnum.Local);
    public static StateStoreLocationSE Remote => FromEnum(StateStoreLocationEnum.Remote);

    private StateStoreLocationSE(StateStoreLocationEnum enumValue, string name) : base(enumValue, name) { }
}

/// <summary>
/// Enumeration for state store locations.
/// </summary>
public enum StateStoreLocationEnum
{
    /// <summary>
    /// Local filesystem storage.
    /// </summary>
    Local = 1,

    /// <summary>
    /// Remote storage (via SSH, network, etc.).
    /// </summary>
    Remote = 2
}