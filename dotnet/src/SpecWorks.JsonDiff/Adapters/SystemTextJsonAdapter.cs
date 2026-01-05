using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json.JsonDiffPatch;
using System.Text.Json.JsonDiffPatch.Diffs.Formatters;

namespace SpecWorks.JsonDiff.Adapters;

/// <summary>
/// Thin adapter that uses SystemTextJson.JsonDiffPatch to generate RFC 6902 patches.
/// </summary>
/// <remarks>
/// This adapter wraps the SystemTextJson.JsonDiffPatch library (v1.x temporary implementation).
/// In v2.x, this will be replaced with a native SpecWorks-generated RFC 6902 implementation.
/// </remarks>
internal class SystemTextJsonAdapter : IJsonDiffGenerator
{
    private readonly JsonPatchDeltaFormatter _formatter = new();

    public JsonPatchDocument CreateDiff(JsonNode source, JsonNode target)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (target == null) throw new ArgumentNullException(nameof(target));

        // Use SystemTextJson.JsonDiffPatch to generate diff in RFC 6902 format
        var patchNode = JsonDiffPatcher.Diff(source, target, _formatter);

        // Convert the JsonNode RFC 6902 patch to JsonPatchDocument
        return ConvertToJsonPatchDocument(patchNode);
    }

    /// <summary>
    /// Converts a RFC 6902 JsonNode patch to Microsoft.AspNetCore.JsonPatch.JsonPatchDocument.
    /// </summary>
    private static JsonPatchDocument ConvertToJsonPatchDocument(JsonNode? patchNode)
    {
        var patch = new JsonPatchDocument();

        if (patchNode == null)
            return patch; // No differences

        // The patch is an array of RFC 6902 operations
        if (patchNode is not JsonArray operations)
            return patch;

        foreach (var operation in operations)
        {
            if (operation is not JsonObject opObj)
                continue;

            var op = opObj["op"]?.GetValue<string>();
            var path = opObj["path"]?.GetValue<string>();

            switch (op?.ToLowerInvariant())
            {
                case "add":
                    var addValue = ConvertJsonNodeValue(opObj["value"]);
                    patch.Add(path!, addValue);
                    break;

                case "remove":
                    patch.Remove(path!);
                    break;

                case "replace":
                    var replaceValue = ConvertJsonNodeValue(opObj["value"]);
                    patch.Replace(path!, replaceValue);
                    break;

                case "move":
                    var from = opObj["from"]?.GetValue<string>();
                    patch.Move(from!, path!);
                    break;

                case "copy":
                    var copyFrom = opObj["from"]?.GetValue<string>();
                    patch.Copy(copyFrom!, path!);
                    break;

                case "test":
                    var testValue = ConvertJsonNodeValue(opObj["value"]);
                    patch.Test(path!, testValue);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown operation type: {op}");
            }
        }

        return patch;
    }

    /// <summary>
    /// Converts a JsonNode value to an appropriate .NET object type.
    /// Preserves JSON type fidelity for integers, strings, booleans, etc.
    /// </summary>
    private static object? ConvertJsonNodeValue(JsonNode? node)
    {
        if (node == null)
            return null;

        if (node is JsonValue value)
        {
            // Try to get the underlying value with proper type
            if (value.TryGetValue(out int intVal))
                return intVal;
            if (value.TryGetValue(out long longVal))
                return longVal;
            if (value.TryGetValue(out double doubleVal))
                return doubleVal;
            if (value.TryGetValue(out bool boolVal))
                return boolVal;
            if (value.TryGetValue(out string? stringVal))
                return stringVal;
        }

        // For arrays and objects, deserialize to .NET types
        return JsonSerializer.Deserialize<object>(node.ToJsonString());
    }
}
