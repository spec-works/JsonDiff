using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.JsonPatch;

namespace SpecWorks.JsonDiff.Tests;

/// <summary>
/// Tests for RFC 6902 compliant serialization.
/// Ensures that serialized output strictly conforms to RFC 6902 specification.
/// </summary>
public class Rfc6902SerializerTests
{
    private readonly IJsonDiffGenerator _generator;

    public Rfc6902SerializerTests()
    {
        _generator = new JsonDiffGenerator();
    }

    [Fact]
    public void Serialize_AddOperation_ContainsOnlyRfc6902Fields()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\"}");
        var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var patch = _generator.CreateDiff(source!, target!);

        // Act
        var json = Rfc6902Serializer.Serialize(patch);
        var operations = JsonSerializer.Deserialize<JsonArray>(json);

        // Assert
        Assert.NotNull(operations);
        Assert.Single(operations);
        
        var operation = operations[0] as JsonObject;
        Assert.NotNull(operation);
        
        // RFC 6902 Section 4.1: add operation has fields: op, path, value
        Assert.Equal("add", operation["op"]!.GetValue<string>());
        Assert.Equal("/age", operation["path"]!.GetValue<string>());
        Assert.Equal(30, operation["value"]!.GetValue<int>());
        
        // Ensure no non-RFC 6902 fields are present
        Assert.Equal(3, operation.Count); // Only op, path, value
        Assert.False(operation.ContainsKey("OperationType"));
        Assert.False(operation.ContainsKey("from"));
    }

    [Fact]
    public void Serialize_RemoveOperation_ContainsOnlyRfc6902Fields()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var target = JsonNode.Parse("{\"name\":\"Alice\"}");
        var patch = _generator.CreateDiff(source!, target!);

        // Act
        var json = Rfc6902Serializer.Serialize(patch);
        var operations = JsonSerializer.Deserialize<JsonArray>(json);

        // Assert
        Assert.NotNull(operations);
        Assert.Single(operations);
        
        var operation = operations[0] as JsonObject;
        Assert.NotNull(operation);
        
        // RFC 6902 Section 4.2: remove operation has fields: op, path
        Assert.Equal("remove", operation["op"]!.GetValue<string>());
        Assert.Equal("/age", operation["path"]!.GetValue<string>());
        
        // Ensure no non-RFC 6902 fields are present
        Assert.Equal(2, operation.Count); // Only op, path
        Assert.False(operation.ContainsKey("OperationType"));
        Assert.False(operation.ContainsKey("value"));
        Assert.False(operation.ContainsKey("from"));
    }

    [Fact]
    public void Serialize_ReplaceOperation_ContainsOnlyRfc6902Fields()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":31}");
        var patch = _generator.CreateDiff(source!, target!);

        // Act
        var json = Rfc6902Serializer.Serialize(patch);
        var operations = JsonSerializer.Deserialize<JsonArray>(json);

        // Assert
        Assert.NotNull(operations);
        Assert.Single(operations);
        
        var operation = operations[0] as JsonObject;
        Assert.NotNull(operation);
        
        // RFC 6902 Section 4.3: replace operation has fields: op, path, value
        Assert.Equal("replace", operation["op"]!.GetValue<string>());
        Assert.Equal("/age", operation["path"]!.GetValue<string>());
        Assert.Equal(31, operation["value"]!.GetValue<int>());
        
        // Ensure no non-RFC 6902 fields are present
        Assert.Equal(3, operation.Count); // Only op, path, value
        Assert.False(operation.ContainsKey("OperationType"));
        Assert.False(operation.ContainsKey("from"));
    }

    [Fact]
    public void Serialize_MultipleOperations_AllAreRfc6902Compliant()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30,\"city\":\"Boston\"}");
        var target = JsonNode.Parse("{\"name\":\"Bob\",\"age\":30,\"country\":\"USA\"}");
        var patch = _generator.CreateDiff(source!, target!);

        // Act
        var json = Rfc6902Serializer.Serialize(patch);
        var operations = JsonSerializer.Deserialize<JsonArray>(json);

        // Assert
        Assert.NotNull(operations);
        Assert.NotEmpty(operations);
        
        foreach (var op in operations!)
        {
            var operation = op as JsonObject;
            Assert.NotNull(operation);
            
            // Every operation must have 'op' and 'path'
            Assert.True(operation.ContainsKey("op"));
            Assert.True(operation.ContainsKey("path"));
            
            // No operation should have OperationType
            Assert.False(operation.ContainsKey("OperationType"));
            
            // Verify field count based on operation type
            var opType = operation["op"]!.GetValue<string>();
            switch (opType)
            {
                case "add":
                case "replace":
                case "test":
                    // These operations have: op, path, value
                    Assert.Equal(3, operation.Count);
                    Assert.True(operation.ContainsKey("value"));
                    break;
                case "remove":
                    // Remove has: op, path
                    Assert.Equal(2, operation.Count);
                    break;
                case "move":
                case "copy":
                    // These operations have: op, from, path
                    Assert.Equal(3, operation.Count);
                    Assert.True(operation.ContainsKey("from"));
                    break;
            }
        }
    }

    [Fact]
    public void Serialize_WithIndentation_FormatsCorrectly()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\"}");
        var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var patch = _generator.CreateDiff(source!, target!);

        // Act
        var json = Rfc6902Serializer.Serialize(patch, writeIndented: true);

        // Assert
        Assert.Contains("\n", json); // Has newlines (indented)
        Assert.Contains("  ", json); // Has indentation
        
        // Verify it's still valid and RFC 6902 compliant
        var operations = JsonSerializer.Deserialize<JsonArray>(json);
        Assert.NotNull(operations);
        Assert.Single(operations);
    }

    [Fact]
    public void Serialize_EmptyPatch_ReturnsEmptyArray()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\"}");
        var target = JsonNode.Parse("{\"name\":\"Alice\"}");
        var patch = _generator.CreateDiff(source!, target!);

        // Act
        var json = Rfc6902Serializer.Serialize(patch);

        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Serialize_NullPatch_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Rfc6902Serializer.Serialize(null!));
    }

    [Fact]
    public void Serialize_ComplexNestedChanges_AllFieldsAreRfc6902Compliant()
    {
        // Arrange
        var source = JsonNode.Parse(@"{
            ""person"": {
                ""name"": ""Alice"",
                ""age"": 30,
                ""address"": {
                    ""city"": ""Boston"",
                    ""state"": ""MA""
                }
            }
        }");
        var target = JsonNode.Parse(@"{
            ""person"": {
                ""name"": ""Alice"",
                ""age"": 31,
                ""address"": {
                    ""city"": ""Cambridge"",
                    ""state"": ""MA"",
                    ""zip"": ""02139""
                }
            }
        }");
        var patch = _generator.CreateDiff(source!, target!);

        // Act
        var json = Rfc6902Serializer.Serialize(patch);
        var operations = JsonSerializer.Deserialize<JsonArray>(json);

        // Assert
        Assert.NotNull(operations);
        Assert.NotEmpty(operations);
        
        // Verify every operation is RFC 6902 compliant
        foreach (var op in operations!)
        {
            var operation = op as JsonObject;
            Assert.NotNull(operation);
            Assert.False(operation.ContainsKey("OperationType"));
        }
    }

    [Fact]
    public void Serialize_WithoutIndentation_IsCompact()
    {
        // Arrange
        var source = JsonNode.Parse("{\"name\":\"Alice\"}");
        var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
        var patch = _generator.CreateDiff(source!, target!);

        // Act
        var json = Rfc6902Serializer.Serialize(patch, writeIndented: false);

        // Assert
        Assert.DoesNotContain("\n  ", json); // No indentation
        
        // Verify it's still valid and RFC 6902 compliant
        var operations = JsonSerializer.Deserialize<JsonArray>(json);
        Assert.NotNull(operations);
        Assert.Single(operations);
    }
}
