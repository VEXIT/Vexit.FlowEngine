/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-02-01 - Activity status enumeration and smart enum wrapper
 *  Date Updated :
 *
 *************************************************************/

using Vexit.Common.BaseClasses;

namespace Vexit.FlowEngine.Enums;

/// <summary>
/// Smart enum wrapper for <see cref="ActivityStatusEnum"/> providing string helpers.
/// Instances are auto-generated from enum values.
/// </summary>
public sealed class ActivityStatusSE : SmartEnumBase<ActivityStatusEnum, ActivityStatusSE>
{
    public static ActivityStatusSE Pending => FromEnum(ActivityStatusEnum.Pending);
    public static ActivityStatusSE Running => FromEnum(ActivityStatusEnum.Running);
    public static ActivityStatusSE Completed => FromEnum(ActivityStatusEnum.Completed);
    public static ActivityStatusSE Failed => FromEnum(ActivityStatusEnum.Failed);
    public static ActivityStatusSE Skipped => FromEnum(ActivityStatusEnum.Skipped);

    private ActivityStatusSE(ActivityStatusEnum value, string name) : base(value, name) { }
}

/// <summary>
/// Activity execution status for persistence and serialization.
/// </summary>
public enum ActivityStatusEnum
{
    Pending,
    Running,
    Completed,
    Failed,
    Skipped
}