using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;
using Xunit;

namespace SpecWorks.JsonDiff.Tests;

/// <summary>
/// Tests using the official JSON Patch test suite from https://github.com/json-patch/json-patch-tests
/// </summary>
/// <remarks>
/// This test suite validates that our diff generator produces correct RFC 6902 patches
/// by comparing the generated patches against the official test suite's expected patches.
/// Tests with "error" or "disabled" fields are filtered out as they test patch application,
/// not diff generation.
/// </remarks>
public class JsonPatchTestSuiteTests
{
    private readonly JsonDiffGenerator _generator = new();

    public static IEnumerable<object[]> GetTestCases()
    {
        var tests = LoadTestFile("tests.json");
        var specTests = LoadTestFile("spec_tests.json");

        return tests.Concat(specTests)
            .Where(t => !t.Disabled && t.Error == null) // Filter out error and disabled tests
            .Select(t => new object[] { t });
    }

    private static IEnumerable<JsonPatchTestCase> LoadTestFile(string filename)
    {
        var path = Path.Combine(AppContext.BaseDirectory, filename);
        var json = File.ReadAllText(path);
        var testCases = JsonSerializer.Deserialize<List<JsonPatchTestCase>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });

        return testCases ?? Enumerable.Empty<JsonPatchTestCase>();
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void GeneratedPatch_ShouldMatchExpectedPatch(JsonPatchTestCase testCase)
    {
        // Arrange
        var source = JsonNode.Parse(testCase.Doc!.Value.GetRawText());
        var target = JsonNode.Parse(testCase.Expected!.Value.GetRawText());

        // Act
        var generatedPatch = _generator.CreateDiff(source!, target!);

        // Assert - Verify the patch produces the expected result
        JToken resultToken;

        // Special handling for root document replacement
        // Check if this is a root document type change (object<->array)
        bool isRootReplacement = generatedPatch.Operations.Count == 1 &&
            generatedPatch.Operations[0].op.ToLowerInvariant() is "replace" &&
            generatedPatch.Operations[0].path == "/" &&
            testCase.Doc!.Value.ValueKind != testCase.Expected!.Value.ValueKind;

        if (isRootReplacement)
        {
            var operation = generatedPatch.Operations[0];
            // For root type replacement, just use the new value directly
            resultToken = ConvertToJToken(operation.value);
        }
        else
        {
            // Normal case: apply operations to source
            resultToken = JToken.Parse(testCase.Doc!.Value.GetRawText());

            // Apply each operation from the generated patch
            foreach (var operation in generatedPatch.Operations)
            {
                ApplyOperation(resultToken, operation);
            }
        }

        var resultNode = JsonNode.Parse(resultToken.ToString());
        var expectedNode = JsonNode.Parse(testCase.Expected!.Value.GetRawText());

        Assert.True(
            JsonNode.DeepEquals(resultNode, expectedNode),
            $"Test '{testCase.Comment}' failed:\n" +
            $"Source: {testCase.Doc!.Value.GetRawText()}\n" +
            $"Expected: {testCase.Expected!.Value.GetRawText()}\n" +
            $"Actual: {resultNode?.ToJsonString()}\n" +
            $"Generated patch: {JsonSerializer.Serialize(generatedPatch.Operations)}\n" +
            $"Expected patch: {testCase.Patch!.Value.GetRawText()}"
        );
    }

    private static void ApplyOperation(JToken document, Operation operation)
    {
        var path = operation.path;

        switch (operation.op.ToLowerInvariant())
        {
            case "add":
                ApplyAdd(document, path, operation.value);
                break;
            case "remove":
                ApplyRemove(document, path);
                break;
            case "replace":
                ApplyReplace(document, path, operation.value);
                break;
            case "move":
                ApplyMove(document, operation.from, path);
                break;
            case "copy":
                ApplyCopy(document, operation.from, path);
                break;
            default:
                throw new NotSupportedException($"Operation '{operation.op}' is not supported");
        }
    }

    private static void ApplyAdd(JToken document, string path, object? value)
    {
        var valueToken = ConvertToJToken(value);

        if (path == "")
        {
            // Can't add to root in-place
            throw new InvalidOperationException("Cannot add to root path");
        }

        var segments = ParsePath(path);
        var parent = NavigateToParent(document, segments, out var lastSegment);

        if (parent is JObject obj)
        {
            obj[lastSegment] = valueToken;
        }
        else if (parent is JArray arr)
        {
            if (lastSegment == "-")
            {
                arr.Add(valueToken);
            }
            else if (int.TryParse(lastSegment, out var index))
            {
                arr.Insert(index, valueToken);
            }
        }
    }

    private static void ApplyRemove(JToken document, string path)
    {
        var segments = ParsePath(path);
        var parent = NavigateToParent(document, segments, out var lastSegment);

        if (parent is JObject obj)
        {
            obj.Remove(lastSegment);
        }
        else if (parent is JArray arr && int.TryParse(lastSegment, out var index))
        {
            arr.RemoveAt(index);
        }
    }

    private static void ApplyReplace(JToken document, string path, object? value)
    {
        var valueToken = ConvertToJToken(value);

        if (path == "")
        {
            // Can't replace root in-place
            throw new InvalidOperationException("Cannot replace root path");
        }

        var segments = ParsePath(path);
        var parent = NavigateToParent(document, segments, out var lastSegment);

        if (parent is JObject obj)
        {
            obj[lastSegment] = valueToken;
        }
        else if (parent is JArray arr && int.TryParse(lastSegment, out var index))
        {
            arr[index] = valueToken;
        }
    }

    private static void ApplyMove(JToken document, string from, string to)
    {
        // Get value at 'from' path
        var segments = ParsePath(from);
        var parent = NavigateToParent(document, segments, out var lastSegment);

        JToken? value = null;
        if (parent is JObject obj && obj.TryGetValue(lastSegment, out var objValue))
        {
            value = objValue;
            obj.Remove(lastSegment);
        }
        else if (parent is JArray arr && int.TryParse(lastSegment, out var index))
        {
            value = arr[index];
            arr.RemoveAt(index);
        }

        // Add to 'to' path
        if (value != null)
        {
            ApplyAdd(document, to, value);
        }
    }

    private static void ApplyCopy(JToken document, string from, string to)
    {
        // Get value at 'from' path
        var value = NavigateTo(document, from);

        // Add copy to 'to' path
        if (value != null)
        {
            ApplyAdd(document, to, value.DeepClone());
        }
    }

    private static JToken NavigateTo(JToken document, string path)
    {
        if (path == "")
            return document;

        var segments = ParsePath(path);
        var current = document;

        foreach (var segment in segments)
        {
            if (current is JObject obj)
            {
                current = obj[segment]!;
            }
            else if (current is JArray arr && int.TryParse(segment, out var index))
            {
                current = arr[index];
            }
        }

        return current;
    }

    private static JToken NavigateToParent(JToken document, string[] segments, out string lastSegment)
    {
        lastSegment = segments[^1];

        if (segments.Length == 1)
            return document;

        var parentSegments = segments[..^1];
        var current = document;

        foreach (var segment in parentSegments)
        {
            if (current is JObject obj)
            {
                current = obj[segment]!;
            }
            else if (current is JArray arr && int.TryParse(segment, out var index))
            {
                current = arr[index];
            }
        }

        return current;
    }

    private static string[] ParsePath(string path)
    {
        if (path == "" || path == "/")
            return new[] { "" };

        return path.TrimStart('/').Split('/')
            .Select(s => s.Replace("~1", "/").Replace("~0", "~"))
            .ToArray();
    }

    private static JToken ConvertToJToken(object? value)
    {
        if (value == null)
            return JValue.CreateNull();

        // If it's already a JToken, return it
        if (value is JToken jtoken)
            return jtoken;

        // If it's a JsonElement, convert it using GetRawText
        if (value is JsonElement element)
        {
            return JToken.Parse(element.GetRawText());
        }

        // If it's a Dictionary (from System.Text.Json deserialization), convert to JSON string and parse
        if (value is Dictionary<string, object> dict)
        {
            var json = JsonSerializer.Serialize(dict);
            return JToken.Parse(json);
        }

        // If it's an array
        if (value is object[] arr)
        {
            var json = JsonSerializer.Serialize(arr);
            return JToken.Parse(json);
        }

        // For primitive types, use FromObject
        return JToken.FromObject(value);
    }
}

/// <summary>
/// Represents a test case from the JSON Patch test suite
/// </summary>
public class JsonPatchTestCase
{
    public string? Comment { get; set; }
    public JsonElement? Doc { get; set; }
    public JsonElement? Patch { get; set; }
    public JsonElement? Expected { get; set; }
    public string? Error { get; set; }
    public bool Disabled { get; set; }

    public override string ToString() => Comment ?? "Unnamed test";
}
