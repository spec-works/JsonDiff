# SpecWorks.JsonDiff (.NET)

RFC 6902 compliant JSON Patch diff generator for .NET.

## Overview

SpecWorks.JsonDiff is a .NET library that generates JSON Patch (RFC 6902) documents by comparing two JSON objects. It provides a simple, standards-focused API that strictly implements RFC 6902 without supporting proprietary formats or extensions.

## Installation

```bash
dotnet add package SpecWorks.JsonDiff
```

Or via NuGet Package Manager:
```
Install-Package SpecWorks.JsonDiff
```

## Quick Start

```csharp
using SpecWorks.JsonDiff;
using System.Text.Json.Nodes;

// Parse JSON documents
var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":31,\"city\":\"NYC\"}");

// Generate RFC 6902 patch
var generator = new JsonDiffGenerator();
var patch = generator.CreateDiff(source, target);

// Result:
// [
//   { "op": "replace", "path": "/age", "value": 31 },
//   { "op": "add", "path": "/city", "value": "NYC" }
// ]

// Apply the patch using Microsoft.AspNetCore.JsonPatch
patch.ApplyTo(source);
// source now equals target
```

## Features

- **RFC 6902 Compliant** - Strictly implements JSON Patch standard
- **Simple API** - Single method: `CreateDiff(source, target)`
- **Type Safe** - Uses `System.Text.Json.Nodes.JsonNode` and `Microsoft.AspNetCore.JsonPatch.JsonPatchDocument`
- **No Configuration** - No format options, always RFC 6902
- **Well Tested** - Comprehensive test suite including RFC examples
- **.NET 10.0 and .NET Standard 2.1** - Supports modern and legacy frameworks

## Serialization

When you need to serialize a patch to JSON (e.g., for API responses), use the `Rfc6902Serializer` to ensure RFC 6902 compliance:

```csharp
using SpecWorks.JsonDiff;
using System.Text.Json.Nodes;

var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":31}");

var generator = new JsonDiffGenerator();
var patch = generator.CreateDiff(source, target);

// Serialize to RFC 6902 compliant JSON
string json = Rfc6902Serializer.Serialize(patch, writeIndented: true);

// Output:
// [
//   {
//     "op": "replace",
//     "path": "/age",
//     "value": 31
//   }
// ]
```

The `Rfc6902Serializer` ensures that only RFC 6902 specified fields are included in the output, excluding internal fields like `OperationType` that are not part of the specification.

## Usage Examples

### Basic Usage

```csharp
using SpecWorks.JsonDiff;
using System.Text.Json.Nodes;

var generator = new JsonDiffGenerator();

// Add property
var source1 = JsonNode.Parse("{\"name\":\"Alice\"}");
var target1 = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
var patch1 = generator.CreateDiff(source1, target1);
// Result: [{ "op": "add", "path": "/age", "value": 30 }]

// Remove property
var source2 = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
var target2 = JsonNode.Parse("{\"name\":\"Alice\"}");
var patch2 = generator.CreateDiff(source2, target2);
// Result: [{ "op": "remove", "path": "/age" }]

// Replace property
var source3 = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
var target3 = JsonNode.Parse("{\"name\":\"Alice\",\"age\":31}");
var patch3 = generator.CreateDiff(source3, target3);
// Result: [{ "op": "replace", "path": "/age", "value": 31 }]
```

### Nested Objects

```csharp
var source = JsonNode.Parse(@"{
    ""person"": {
        ""name"": ""Alice"",
        ""age"": 30
    }
}");

var target = JsonNode.Parse(@"{
    ""person"": {
        ""name"": ""Alice"",
        ""age"": 31
    }
}");

var patch = generator.CreateDiff(source, target);
// Result: [{ "op": "replace", "path": "/person/age", "value": 31 }]
```

### Complex Updates

```csharp
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

var patch = generator.CreateDiff(source, target);
// Result: Multiple operations (replace age, remove city, add country, replace active)
```

### Dependency Injection

```csharp
// Startup.cs or Program.cs
services.AddSingleton<IJsonDiffGenerator, JsonDiffGenerator>();

// Usage in a class
public class MyService
{
    private readonly IJsonDiffGenerator _diffGenerator;

    public MyService(IJsonDiffGenerator diffGenerator)
    {
        _diffGenerator = diffGenerator;
    }

    public void CompareDocuments(JsonNode source, JsonNode target)
    {
        var patch = _diffGenerator.CreateDiff(source, target);
        // Use patch...
    }
}
```

## RFC 6902 Operations

JSON Patch supports six operation types:

| Operation | Description | Example |
|-----------|-------------|---------|
| `add` | Add a value to an object or array | `{"op":"add","path":"/name","value":"Alice"}` |
| `remove` | Remove a value | `{"op":"remove","path":"/name"}` |
| `replace` | Replace a value | `{"op":"replace","path":"/age","value":31}` |
| `move` | Move a value | `{"op":"move","from":"/old","path":"/new"}` |
| `copy` | Copy a value | `{"op":"copy","from":"/orig","path":"/copy"}` |
| `test` | Test that a value equals specified | `{"op":"test","path":"/name","value":"Alice"}` |

## What This Library Does NOT Support

In accordance with the SpecWorks design philosophy, this library intentionally does **NOT** support:

- ❌ **RFC 7386 (JSON Merge Patch)** - Different specification
- ❌ **Proprietary diff formats** - Vendor-specific formats
- ❌ **Non-standard extensions** - Custom operations
- ❌ **Format configuration options** - RFC 6902 only, always

This constraint ensures:
- **Interoperability** - Works with any RFC 6902 compliant system
- **Standards compliance** - Predictable, specification-based behavior
- **Simplicity** - No configuration needed
- **Future-proof** - Easy migration to native implementation (v2.x)

## Implementation Notes

### Version 1.x (Current)

The current implementation uses a **wrapper architecture** with `SystemTextJson.JsonDiffPatch` as the underlying library. This approach:

- ✅ Provides immediate RFC 6902 functionality
- ✅ Uses battle-tested external library
- ✅ Hides implementation details behind stable interface
- ✅ Enables future migration without breaking changes

### Version 2.x (Future)

Future versions will replace the external dependency with a **native SpecWorks-generated RFC 6902 implementation**. The public API (`IJsonDiffGenerator`) will remain unchanged, ensuring seamless migration for all consumers.

## API Reference

### IJsonDiffGenerator Interface

```csharp
public interface IJsonDiffGenerator
{
    JsonPatchDocument CreateDiff(JsonNode source, JsonNode target);
}
```

**CreateDiff Method**

Generates an RFC 6902 JSON Patch document representing the differences between source and target.

- **Parameters:**
  - `source` - The source JSON document (JsonNode)
  - `target` - The target JSON document (JsonNode)
- **Returns:** `JsonPatchDocument` containing RFC 6902 operations
- **Throws:** `ArgumentNullException` if source or target is null

### JsonDiffGenerator Class

```csharp
public class JsonDiffGenerator : IJsonDiffGenerator
{
    public JsonDiffGenerator();
    public JsonPatchDocument CreateDiff(JsonNode source, JsonNode target);
}
```

Default implementation of `IJsonDiffGenerator`.

### Rfc6902Serializer Class

```csharp
public static class Rfc6902Serializer
{
    public static string Serialize(JsonPatchDocument patch, bool writeIndented = false);
}
```

**Serialize Method**

Serializes a JsonPatchDocument to an RFC 6902 compliant JSON string.

- **Parameters:**
  - `patch` - The patch document to serialize (JsonPatchDocument)
  - `writeIndented` - Whether to format the output with indentation (default: false)
- **Returns:** RFC 6902 compliant JSON string
- **Throws:** `ArgumentNullException` if patch is null

**Why use Rfc6902Serializer?**

The `JsonPatchDocument.Operations` collection contains `Microsoft.AspNetCore.JsonPatch.Operations.Operation` objects which have internal properties like `OperationType` that are not part of RFC 6902. Using `Rfc6902Serializer.Serialize()` ensures:

- Only RFC 6902 specified fields are included
- No internal implementation details leak into output
- Full specification compliance for interoperability

## Testing

The library includes comprehensive tests covering:

- ✅ All RFC 6902 operation types
- ✅ RFC examples from specification
- ✅ All JSON data types (string, number, boolean, null, array, object)
- ✅ Edge cases (empty objects, deep nesting)
- ✅ Complex real-world scenarios
- ✅ Null argument validation

Run tests:

```bash
cd tests/SpecWorks.JsonDiff.Tests
dotnet test
```

## Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/spec-works.git
cd spec-works/JsonDiff/dotnet

# Build
dotnet build

# Run tests
dotnet test

# Pack NuGet package
dotnet pack src/SpecWorks.JsonDiff/SpecWorks.JsonDiff.csproj
```

## Project Structure

```
dotnet/
├── src/
│   └── SpecWorks.JsonDiff/
│       ├── IJsonDiffGenerator.cs        # Public interface
│       ├── JsonDiffGenerator.cs         # Public implementation
│       ├── Adapters/
│       │   └── SystemTextJsonAdapter.cs # Internal adapter (v1.x)
│       └── SpecWorks.JsonDiff.csproj
├── tests/
│   └── SpecWorks.JsonDiff.Tests/
│       ├── JsonDiffGeneratorTests.cs
│       └── SpecWorks.JsonDiff.Tests.csproj
└── README.md
```

## Dependencies

- **System.Text.Json** - Included in .NET
- **Microsoft.AspNetCore.JsonPatch** - For JsonPatchDocument type
- **SystemTextJson.JsonDiffPatch** - Internal implementation (v1.x only)

## Compatibility

- **.NET 10.0** - Full support
- **.NET Standard 2.1** - Full support for legacy projects

## Performance Considerations

The library is designed for correctness and standards compliance. For most use cases, performance is excellent. If you're diffing very large documents (>1MB), consider:

- Streaming comparisons (custom implementation)
- Chunking large documents
- Caching diff results

## Contributing

Contributions are welcome! Please:

1. Review the [Architecture Decision Records](../adr/)
2. Follow RFC 6902 strictly
3. Add tests for new functionality
4. Maintain backward compatibility

## License

MIT License - See LICENSE file for details.

## References

- [RFC 6902 - JavaScript Object Notation (JSON) Patch](https://datatracker.ietf.org/doc/html/rfc6902)
- [RFC 6901 - JSON Pointer](https://datatracker.ietf.org/doc/html/rfc6901)
- [JSON Patch Website](https://jsonpatch.com/)
- [SpecWorks Vision](../../VISION.md)
- [Architecture Decision Records](../adr/)

## Related Projects

This JSON Patch generator is part of the [SpecWorks](../../) collection of specification-based software components.
