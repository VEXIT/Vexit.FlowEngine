/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Activity type enumeration
 *  Date Updated :
 *
 *************************************************************/

using Vexit.Common.BaseClasses;

namespace Vexit.FlowEngine.Enums;

public sealed class ActivityTypeSE : SmartEnumBase<ActivityTypeEnum, ActivityTypeSE>
{
    public static ActivityTypeSE Step => FromEnum(ActivityTypeEnum.Step);
    public static ActivityTypeSE Group => FromEnum(ActivityTypeEnum.Group);
    public static ActivityTypeSE Decision => FromEnum(ActivityTypeEnum.Decision);

    private ActivityTypeSE(ActivityTypeEnum enumValue, string name) : base(enumValue, name)
    {
    }
}

public enum ActivityTypeEnum
{
    Step,       // Executable leaf activity (does work)
    Group,      // Container for sequential/parallel activities
    Decision    // Branching logic based on conditions
}