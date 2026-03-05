/*************************************************************
 *
 *  Copyright    : © VEXIT 2026, www.vexit.com
 *  Author       : Vex Tatarevic
 *  Date Created : 2026-01-24 - File-based state store implementation
 *  Date Updated :
 *
 *************************************************************/

using System.Text.Json;
using Vexit.FlowEngine.Core.Abstractions;
using Vexit.FlowEngine.Models;
using Vexit.FlowEngine.Utils;

namespace Vexit.FlowEngine.Runtime;

/// <summary>
/// Default file-based state store for flow instances and activity records.
/// </summary>
public sealed class StateStore : IStateStore
{

    //------------------------------------------------------------
    //      FIELDS
    //------------------------------------------------------------
    private readonly FlowEngineConfig _config;
    private readonly string _rootPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    //------------------------------------------------------------
    //      CONSTRUCTOR
    //------------------------------------------------------------
    public StateStore(FlowEngineConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _rootPath = Util.ResolvePath(config.StateStoreBasePath);
    }

    //------------------------------------------------------------
    //      METHODS
    //------------------------------------------------------------
    public async Task SaveFlowInstanceAsync(FlowInstance instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        var path = GetInstancePath(instance.Id);
        EnsureDirectory(Path.GetDirectoryName(path));

        var json = JsonSerializer.Serialize(instance, _jsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<FlowInstance?> LoadFlowInstanceAsync(string flowInstanceId)
    {
        var path = GetInstancePath(flowInstanceId);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<FlowInstance>(json);
    }

    private string GetInstancePath(string id)
    {
        ValidateInstanceId(id);
        var fileName = Util.GetFlowInstanceFileName(id);
        return Path.Combine(_rootPath, fileName);
    }

    private static void ValidateInstanceId(string instanceId)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            throw new ArgumentException("InstanceId must be provided.", nameof(instanceId));
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var invalidCharsFound = instanceId.Where(c => invalidChars.Contains(c)).Distinct().ToList();
        if (invalidCharsFound.Count > 0)
        {
            var invalidCharList = string.Join(", ", invalidCharsFound.Select(c => $"'{c}'"));
            throw new ArgumentException(
                $"InstanceId contains invalid filename characters: {invalidCharList}.",
                nameof(instanceId));
        }
    }

    private static void EnsureDirectory(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
    }
}

