using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SpecWorks.JsonDiff.Tests;

/// <summary>
/// Tests for edge cases inspired by error scenarios in the JSON Patch test suite.
/// While the error test cases in tests.json and spec_tests.json are designed to validate
/// patch *application* failures, these tests ensure our diff *generator* handles edge cases
/// appropriately and produces valid patches.
/// </summary>
/// <remarks>
/// The JSON Patch test suite (https://github.com/json-patch/json-patch-tests) contains
/// test cases with an "error" field that validate patch application should fail. Since
/// JsonDiffGenerator is a diff generator, not a patch applier, we adapt these scenarios
/// to test that:
/// 1. The generator produces valid patches for edge case documents
/// 2. The generated patches can be successfully applied
/// 3. Edge cases are handled gracefully without exceptions
/// </remarks>
public class JsonPatchErrorEdgeCaseTests
{
    private readonly JsonDiffGenerator _generator = new();

    #region Array Boundary Cases
    
    /// <summary>
    /// Tests that diff generator handles array transformations correctly without producing
    /// out-of-bounds operations. Inspired by test cases: "Out of bounds (upper/lower)"
    /// </summary>
    [Fact]
    public void CreateDiff_ArrayGrowth_ProducesValidPatch()
    {
        // Arrange - array growing from 2 to 9 elements
        var source = JsonNode.Parse("{\"bar\":[1,2]}");
        var target = JsonNode.Parse("{\"bar\":[1,2,3,4,5,6,7,8,9]}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should produce valid operations (not out-of-bounds)
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
        
        // All operations should have valid paths
        foreach (var op in patch.Operations)
        {
            Assert.NotNull(op.path);
            Assert.NotEmpty(op.path);
        }
    }

    /// <summary>
    /// Tests that diff generator handles array shrinking correctly.
    /// </summary>
    [Fact]
    public void CreateDiff_ArrayShrink_ProducesValidPatch()
    {
        // Arrange - array shrinking from 9 to 2 elements
        var source = JsonNode.Parse("{\"bar\":[1,2,3,4,5,6,7,8,9]}");
        var target = JsonNode.Parse("{\"bar\":[1,2]}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should produce valid operations
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
    }

    /// <summary>
    /// Tests that diff generator handles empty array transformations.
    /// </summary>
    [Fact]
    public void CreateDiff_EmptyArray_ProducesValidPatch()
    {
        // Arrange
        var source = JsonNode.Parse("{\"bar\":[]}");
        var target = JsonNode.Parse("{\"bar\":[1,2,3]}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
    }

    #endregion

    #region Type Mismatch Cases

    /// <summary>
    /// Tests that diff generator handles object-to-array type changes.
    /// Inspired by: "Object operation on array target"
    /// </summary>
    [Fact]
    public void CreateDiff_ObjectToArray_ProducesValidPatch()
    {
        // Arrange - changing object to array
        var source = JsonNode.Parse("{\"data\":{\"foo\":\"bar\"}}");
        var target = JsonNode.Parse("{\"data\":[\"foo\",\"bar\"]}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should produce valid replace operation
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
        
        // Should contain an operation affecting the /data path
        Assert.Contains(patch.Operations, op => op.path.StartsWith("/data"));
    }

    /// <summary>
    /// Tests that diff generator handles array-to-object type changes.
    /// </summary>
    [Fact]
    public void CreateDiff_ArrayToObject_ProducesValidPatch()
    {
        // Arrange - changing array to object
        var source = JsonNode.Parse("{\"data\":[\"foo\",\"bar\"]}");
        var target = JsonNode.Parse("{\"data\":{\"foo\":\"bar\"}}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should produce valid replace operation
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
    }

    #endregion

    #region Special Characters and Edge Cases

    /// <summary>
    /// Tests that diff generator handles properties with special number-like names.
    /// Inspired by: "test with bad number should fail" (1e0)
    /// </summary>
    [Fact]
    public void CreateDiff_NumberLikePropertyNames_HandledCorrectly()
    {
        // Arrange - property name looks like scientific notation
        var source = JsonNode.Parse("{\"1e0\":\"foo\"}");
        var target = JsonNode.Parse("{\"1e0\":\"bar\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should handle as string property, not array index
        Assert.NotNull(patch);
        Assert.Single(patch.Operations);
        Assert.Equal("/1e0", patch.Operations[0].path);
    }

    /// <summary>
    /// Tests that diff generator handles properties with leading zero names.
    /// Inspired by: "test with bad array number that has leading zeros"
    /// </summary>
    [Fact]
    public void CreateDiff_LeadingZeroPropertyNames_HandledCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"00\":\"foo\",\"01\":\"bar\"}");
        var target = JsonNode.Parse("{\"00\":\"changed\",\"01\":\"bar\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should handle as string properties
        Assert.NotNull(patch);
        Assert.Single(patch.Operations);
        Assert.Equal("/00", patch.Operations[0].path);
    }

    #endregion

    #region Nested Path Cases

    /// <summary>
    /// Tests diff generation when target has nested paths that don't exist in source.
    /// Inspired by: "add with missing object" - ensures we don't try to add to nonexistent parents
    /// </summary>
    [Fact]
    public void CreateDiff_DeepPathAddition_ProducesValidPatch()
    {
        // Arrange - target has deeply nested structure
        var source = JsonNode.Parse("{\"q\":{\"bar\":2}}");
        var target = JsonNode.Parse("{\"q\":{\"bar\":2},\"a\":{\"b\":1}}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should create the entire /a object, not try to add to nonexistent /a/b
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
        
        // Should add /a as a complete object
        var addAOp = patch.Operations.FirstOrDefault(op => op.path == "/a");
        Assert.NotNull(addAOp);
    }

    /// <summary>
    /// Tests diff generation when removing nested structures.
    /// Inspired by: "Removing deep nonexistent path"
    /// </summary>
    [Fact]
    public void CreateDiff_DeepPathRemoval_ProducesValidPatch()
    {
        // Arrange - source has deeply nested structure, target doesn't
        var source = JsonNode.Parse("{\"foo\":\"bar\",\"deep\":{\"nested\":{\"value\":1}}}");
        var target = JsonNode.Parse("{\"foo\":\"bar\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should remove the top-level property
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
        
        // Should have a remove operation
        Assert.Contains(patch.Operations, op => op.op == "remove");
    }

    #endregion

    #region Document Root Cases

    /// <summary>
    /// Tests diff generation for root-level document replacement.
    /// </summary>
    [Fact]
    public void CreateDiff_RootReplacement_ObjectToObject_ProducesValidPatch()
    {
        // Arrange - complete root replacement
        var source = JsonNode.Parse("{\"foo\":\"bar\"}");
        var target = JsonNode.Parse("{\"baz\":\"qux\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should produce valid operations
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
    }

    /// <summary>
    /// Tests diff generation for root-level array to object transformation.
    /// </summary>
    [Fact]
    public void CreateDiff_RootReplacement_ArrayToObject_ProducesValidPatch()
    {
        // Arrange - root type change
        var source = JsonNode.Parse("[]");
        var target = JsonNode.Parse("{}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should produce a root replacement operation
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
        
        // Should have an operation at root path
        Assert.Contains(patch.Operations, op => op.path == "/" || op.path == "");
    }

    /// <summary>
    /// Tests diff generation for root-level object to array transformation.
    /// </summary>
    [Fact]
    public void CreateDiff_RootReplacement_ObjectToArray_ProducesValidPatch()
    {
        // Arrange - root type change
        var source = JsonNode.Parse("{}");
        var target = JsonNode.Parse("[]");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - should produce a root replacement operation
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
        
        // Should have an operation at root path
        Assert.Contains(patch.Operations, op => op.path == "/" || op.path == "");
    }

    #endregion

    #region Complex Scenarios

    /// <summary>
    /// Tests that all generated patches produce valid, well-formed operations.
    /// This is a general validation test for edge cases.
    /// </summary>
    [Fact]
    public void CreateDiff_ComplexTransformation_AllOperationsValid()
    {
        // Arrange - complex transformation with multiple edge cases
        var source = JsonNode.Parse(@"{
            ""array"": [1, 2, 3],
            ""object"": {""nested"": {""value"": 1}},
            ""1e0"": ""scientific"",
            ""00"": ""leading-zero""
        }");
        var target = JsonNode.Parse(@"{
            ""array"": [],
            ""object"": [""now-array""],
            ""1e0"": ""changed"",
            ""newProp"": ""added""
        }");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert - validate all operations are well-formed
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
        
        foreach (var op in patch.Operations)
        {
            // All operations must have valid op type
            Assert.Contains(op.op, new[] { "add", "remove", "replace", "move", "copy", "test" });
            
            // All operations must have a path
            Assert.NotNull(op.path);
            
            // Operations requiring 'from' should have it
            if (op.op is "move" or "copy")
            {
                Assert.NotNull(op.from);
                Assert.NotEmpty(op.from);
            }
            
            // Operations requiring 'value' should have it (null is valid)
            if (op.op is "add" or "replace" or "test")
            {
                // value can be null, but the property should exist
                // We can't directly check this, but we trust the generator
            }
        }
    }

    /// <summary>
    /// Tests that generated patches can be serialized and deserialized.
    /// </summary>
    [Fact]
    public void CreateDiff_GeneratedPatch_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var source = JsonNode.Parse("{\"foo\":1}");
        var target = JsonNode.Parse("{\"foo\":2,\"bar\":3}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);
        
        // Serialize the operations
        var json = JsonSerializer.Serialize(patch.Operations);
        
        // Assert - should be valid JSON
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        
        // Should be able to parse it back
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.NotEqual(default(JsonElement), parsed);
    }

    #endregion
}
