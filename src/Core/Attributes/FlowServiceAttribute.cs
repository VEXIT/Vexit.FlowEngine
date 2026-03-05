/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-17 - Attribute for declaring flow services
 *  Date Updated : 2026-01-17 - Changed to generic syntax
 *
 *************************************************************/

namespace Vexit.FlowEngine.Core.Attributes;

/// <summary>
/// Declares the main service type containing all activity methods for this flow. <br/>
/// Applied to flow classes to indicate which service should be scanned for [Activity] methods. <br/>
/// The service can inject other dependencies internally via normal DI.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FlowServiceAttribute<TService> : Attribute
{
    /// <summary>
    /// The service type to scan for activities.
    /// </summary>
    public Type ServiceType => typeof(TService);
}