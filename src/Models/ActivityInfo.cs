/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-31 - Activity information model for observers
 *  Date Updated :
 *
 *************************************************************/

using Vexit.FlowEngine.Enums;

namespace Vexit.FlowEngine.Models;

/// <summary>
/// Information about an activity for observer notifications. <br />
/// Provides all activity metadata without requiring individual parameters.
/// </summary>
public class ActivityInfo
{
    /// <summary>
    /// The unique activity key (e.g., "server.setup.ufw_firewall").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable activity name (e.g., "Setup UFW Firewall").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The activity type.
    /// </summary>
    public ActivityTypeSE Type { get; set; } = ActivityTypeSE.Step;

    /// <summary>
    /// Optional description of the activity.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional category for grouping activities.
    /// </summary>
    public string? Category { get; set; }
}