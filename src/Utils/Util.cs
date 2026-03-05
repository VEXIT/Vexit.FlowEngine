/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-02-07 - Utility methods
 *  Date Updated :
 *
 *************************************************************/

using System;
using System.IO;

namespace Vexit.FlowEngine.Utils;

/// <summary>
/// General utility methods for FlowEngine.
/// </summary>
public static class Util
{
    /// <summary>
    /// Resolves path placeholders like ~ to actual paths.
    /// </summary>
    public static string ResolvePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        // Resolve ~ to user home directory
        if (path.StartsWith("~/") || path.StartsWith("~\\"))
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDir, path.Substring(2));
        }

        // Return as-is for absolute paths or relative paths
        return Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
    }

    /// <summary>
    /// Generates a filename for a flow instance state file.
    /// </summary>
    public static string GetFlowInstanceFileName(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            throw new ArgumentException("InstanceId must be provided.", nameof(instanceId));
        }

        return $"{instanceId}.json";
    }
}