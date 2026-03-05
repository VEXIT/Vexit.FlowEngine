/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-29 - State store type smart enum
 *  Date Updated :
 *
 *************************************************************/

using Vexit.Common.BaseClasses;

namespace Vexit.FlowEngine.Enums;

/// <summary>
/// Smart enum wrapper for <see cref="StateStoreTypeEnum"/> providing string helpers.
/// Instances are auto-generated from enum values.
/// </summary>
public sealed class StateStoreTypeSE : SmartEnumBase<StateStoreTypeEnum, StateStoreTypeSE>
{
    public static StateStoreTypeSE File => FromEnum(StateStoreTypeEnum.File);
    public static StateStoreTypeSE Database => FromEnum(StateStoreTypeEnum.Database);

    private StateStoreTypeSE(StateStoreTypeEnum enumValue, string name) : base(enumValue, name) { }
}

/// <summary>
/// Enumeration for state store types.
/// </summary>
public enum StateStoreTypeEnum
{
    /// <summary>
    /// File-based storage (JSON files).
    /// </summary>
    File = 1,

    /// <summary>
    /// Database-based storage.
    /// </summary>
    Database = 2
}