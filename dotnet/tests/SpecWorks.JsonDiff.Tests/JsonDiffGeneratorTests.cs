using System.Text.Json.Nodes;
using Microsoft.AspNetCore.JsonPatch;

namespace SpecWorks.JsonDiff.Tests;

/// <summary>
/// Tests for JsonDiffGenerator RFC 6902 compliance.
/// </summary>
public class JsonDiffGeneratorTests
{
    private readonly IJsonDiffGenerator _generator;

    public JsonDiffGeneratorTests()
    {
        _generator = new JsonDiffGenerator();
    }

    #region Basic Operations

    [Fact]
    public void CreateDiff_AddProperty_GeneratesAddOperation()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\"}");
        var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.Single(patch.Operations);
        Assert.Equal("add", patch.Operations[0].op);
        Assert.Equal("/age", patch.Operations[0].path);
        Assert.Equal(30, patch.Operations[0].value);
    }

    [Fact]
    public void CreateDiff_RemoveProperty_GeneratesRemoveOperation()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var target = JsonNode.Parse("{\"name\":\"Alice\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.Single(patch.Operations);
        Assert.Equal("remove", patch.Operations[0].op);
        Assert.Equal("/age", patch.Operations[0].path);
    }

    [Fact]
    public void CreateDiff_ReplaceProperty_GeneratesReplaceOperation()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":31}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.Single(patch.Operations);
        Assert.Equal("replace", patch.Operations[0].op);
        Assert.Equal("/age", patch.Operations[0].path);
        Assert.Equal(31, patch.Operations[0].value);
    }

    [Fact]
    public void CreateDiff_NoChanges_GeneratesEmptyPatch()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.Empty(patch.Operations);
    }

    [Fact]
    public void CreateDiff_MultipleChanges_GeneratesMultipleOperations()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var target = JsonNode.Parse("{\"name\":\"Bob\",\"age\":30,\"city\":\"NYC\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.True(patch.Operations.Count >= 2); // At least replace + add
    }

    #endregion

    #region RFC 6902 Examples

    [Fact]
    public void CreateDiff_RFC6902_Example1_AddObjectMember()
    {
        // RFC 6902 Appendix A.1: Adding an Object Member
        // Arrange
        var source = JsonNode.Parse("{\"foo\":\"bar\"}");
        var target = JsonNode.Parse("{\"foo\":\"bar\",\"baz\":\"qux\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var addOp = patch.Operations.FirstOrDefault(o => o.op == "add" && o.path == "/baz");
        Assert.NotNull(addOp);
        Assert.Equal("qux", addOp.value);
    }

    [Fact]
    public void CreateDiff_RFC6902_Example2_AddArrayElement()
    {
        // RFC 6902 Appendix A.2: Adding an Array Element
        // Arrange
        var source = JsonNode.Parse("{\"foo\":[\"bar\",\"baz\"]}");
        var target = JsonNode.Parse("{\"foo\":[\"bar\",\"qux\",\"baz\"]}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
        // Should contain an add operation for array
    }

    [Fact]
    public void CreateDiff_RFC6902_Example3_RemoveObjectMember()
    {
        // RFC 6902 Appendix A.3: Removing an Object Member
        // Arrange
        var source = JsonNode.Parse("{\"foo\":\"bar\",\"baz\":\"qux\"}");
        var target = JsonNode.Parse("{\"foo\":\"bar\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var removeOp = patch.Operations.FirstOrDefault(o => o.op == "remove" && o.path == "/baz");
        Assert.NotNull(removeOp);
    }

    [Fact]
    public void CreateDiff_RFC6902_Example5_ReplaceValue()
    {
        // RFC 6902 Appendix A.5: Replacing a Value
        // Arrange
        var source = JsonNode.Parse("{\"foo\":\"bar\",\"baz\":\"qux\"}");
        var target = JsonNode.Parse("{\"foo\":\"bar\",\"baz\":\"boo\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var replaceOp = patch.Operations.FirstOrDefault(o => o.op == "replace" && o.path == "/baz");
        Assert.NotNull(replaceOp);
        Assert.Equal("boo", replaceOp.value);
    }

    #endregion

    #region Data Types

    [Fact]
    public void CreateDiff_StringValue_HandledCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"text\":\"hello\"}");
        var target = JsonNode.Parse("{\"text\":\"world\"}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var replaceOp = patch.Operations.FirstOrDefault(o => o.path == "/text");
        Assert.NotNull(replaceOp);
        Assert.Equal("world", replaceOp.value);
    }

    [Fact]
    public void CreateDiff_NumberValue_HandledCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"count\":42}");
        var target = JsonNode.Parse("{\"count\":100}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var replaceOp = patch.Operations.FirstOrDefault(o => o.path == "/count");
        Assert.NotNull(replaceOp);
        Assert.Equal(100, Convert.ToInt32(replaceOp.value));
    }

    [Fact]
    public void CreateDiff_BooleanValue_HandledCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"active\":true}");
        var target = JsonNode.Parse("{\"active\":false}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var replaceOp = patch.Operations.FirstOrDefault(o => o.path == "/active");
        Assert.NotNull(replaceOp);
        Assert.Equal(false, replaceOp.value);
    }

    [Fact]
    public void CreateDiff_NullValue_HandledCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"data\":\"value\"}");
        var target = JsonNode.Parse("{\"data\":null}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var replaceOp = patch.Operations.FirstOrDefault(o => o.path == "/data");
        Assert.NotNull(replaceOp);
        Assert.Null(replaceOp.value);
    }

    [Fact]
    public void CreateDiff_ArrayValue_HandledCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"items\":[1,2,3]}");
        var target = JsonNode.Parse("{\"items\":[1,2,3,4]}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);
    }

    [Fact]
    public void CreateDiff_NestedObject_HandledCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"person\":{\"name\":\"Alice\",\"age\":30}}");
        var target = JsonNode.Parse("{\"person\":{\"name\":\"Alice\",\"age\":31}}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var replaceOp = patch.Operations.FirstOrDefault(o => o.path == "/person/age");
        Assert.NotNull(replaceOp);
        Assert.Equal(31, Convert.ToInt32(replaceOp.value));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CreateDiff_EmptyObjects_GeneratesEmptyPatch()
    {
        // Arrange
        var source = JsonNode.Parse("{}");
        var target = JsonNode.Parse("{}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.Empty(patch.Operations);
    }

    [Fact]
    public void CreateDiff_EmptyToPopulated_GeneratesAddOperations()
    {
        // Arrange
        var source = JsonNode.Parse("{}");
        var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.True(patch.Operations.Count >= 2);
        Assert.All(patch.Operations, op => Assert.Equal("add", op.op));
    }

    [Fact]
    public void CreateDiff_PopulatedToEmpty_GeneratesRemoveOperations()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var target = JsonNode.Parse("{}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.True(patch.Operations.Count >= 2);
        Assert.All(patch.Operations, op => Assert.Equal("remove", op.op));
    }

    [Fact]
    public void CreateDiff_DeepNesting_HandledCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"a\":{\"b\":{\"c\":{\"d\":1}}}}");
        var target = JsonNode.Parse("{\"a\":{\"b\":{\"c\":{\"d\":2}}}}");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        var replaceOp = patch.Operations.FirstOrDefault(o => o.path == "/a/b/c/d");
        Assert.NotNull(replaceOp);
        Assert.Equal(2, Convert.ToInt32(replaceOp.value));
    }

    #endregion

    #region Null Checks

    [Fact]
    public void CreateDiff_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var target = JsonNode.Parse("{\"name\":\"Alice\"}");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _generator.CreateDiff(null!, target!));
    }

    [Fact]
    public void CreateDiff_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\"}");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _generator.CreateDiff(source!, null!));
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void CreateDiff_MixedOperations_GeneratesCorrectPatch()
    {
        // Arrange - combination of add, remove, replace
        var source = JsonNode.Parse(@"{
            ""name"": ""Alice"",
            ""age"": 30,
            ""city"": ""Boston"",
            ""active"": true
        }");
        var target = JsonNode.Parse(@"{
            ""name"": ""Alice"",
            ""age"": 31,
            ""country"": ""USA"",
            ""active"": false
        }");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.True(patch.Operations.Count >= 3);

        // Should have replace for age
        Assert.Contains(patch.Operations, o => o.op == "replace" && o.path == "/age");

        // Should have remove for city
        Assert.Contains(patch.Operations, o => o.op == "remove" && o.path == "/city");

        // Should have add for country
        Assert.Contains(patch.Operations, o => o.op == "add" && o.path == "/country");

        // Should have replace for active
        Assert.Contains(patch.Operations, o => o.op == "replace" && o.path == "/active");
    }

    [Fact]
    public void CreateDiff_RealWorldExample_ContactUpdate()
    {
        // Arrange - realistic contact update scenario
        var source = JsonNode.Parse(@"{
            ""id"": ""123"",
            ""name"": ""John Doe"",
            ""email"": ""john@example.com"",
            ""phone"": ""+1-555-1234"",
            ""address"": {
                ""street"": ""123 Main St"",
                ""city"": ""Boston"",
                ""state"": ""MA""
            }
        }");

        var target = JsonNode.Parse(@"{
            ""id"": ""123"",
            ""name"": ""John Doe"",
            ""email"": ""john.doe@newdomain.com"",
            ""phone"": ""+1-555-1234"",
            ""address"": {
                ""street"": ""456 Oak Ave"",
                ""city"": ""Boston"",
                ""state"": ""MA"",
                ""zip"": ""02101""
            },
            ""verified"": true
        }");

        // Act
        var patch = _generator.CreateDiff(source!, target!);

        // Assert
        Assert.NotNull(patch);
        Assert.NotEmpty(patch.Operations);

        // Email should be replaced
        Assert.Contains(patch.Operations, o => o.path == "/email");

        // Address street should be updated
        Assert.Contains(patch.Operations, o => o.path == "/address/street");

        // Zip should be added
        Assert.Contains(patch.Operations, o => o.path == "/address/zip");

        // Verified should be added
        Assert.Contains(patch.Operations, o => o.path == "/verified");
    }

    #endregion
}
