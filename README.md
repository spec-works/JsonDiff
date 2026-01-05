# JsonDiff - RFC 6902 JSON Patch Generator

A specification-compliant library for generating JSON Patch (RFC 6902) documents from two JSON objects.

## Status

**Phase**: .NET v1.0 Complete
**Version**: v1.0.0 (.NET), Python (planned), Rust (planned)
**Languages**: .NET ✅ | Python ⏳ | Rust ⏳

## Overview

JsonDiff is a SpecWorks component that generates RFC 6902 compliant JSON Patch documents by comparing two JSON documents. It provides a simple, standards-focused API that enables applications to:

- Generate diff patches between JSON documents
- Apply patches using standard JSON Patch implementations
- Ensure interoperability through strict RFC 6902 compliance
- Avoid vendor lock-in by rejecting proprietary formats

## Design Philosophy

This component follows three core principles:

1. **RFC 6902 Only** - Strictly implements RFC 6902, no other formats
2. **Wrapper Architecture** - Uses external libraries temporarily (v1.x), with migration path to native implementation (v2.x)
3. **Multi-Language** - Consistent APIs across .NET, Python, and Rust

See [Architecture Decision Records](adr/) for detailed rationale.

## Specifications

This implementation is based on:

- **[RFC 6902](https://datatracker.ietf.org/doc/html/rfc6902)** - JavaScript Object Notation (JSON) Patch (April 2013)

See [specs.json](specs.json) for the full specification linkset.

## Features

### Version 1.x (Current - .NET Implemented)

- **RFC 6902 Diff Generation** ✅
  - Compare two JSON documents
  - Generate standard patch operations: add, remove, replace, move, copy, test
  - Output as JsonPatchDocument (.NET) or standard patch format

- **Strict Specification Compliance** ✅
  - No proprietary formats
  - No RFC 7386 (JSON Merge Patch) support
  - No non-standard extensions
  - RFC 6902 operations only

- **Simple API** ✅
  - Single method: `CreateDiff(source, target)`
  - No configuration options
  - No format parameters

### Version 2.x (Future - Native Implementation)

- **SpecWorks-Generated Code**
  - Remove external library dependencies
  - Native RFC 6902 implementation
  - Consumer API unchanged

## Usage

### .NET (Available Now)

**Installation:**
```bash
dotnet add package SpecWorks.JsonDiff
```

**Quick Start:**
```csharp
using SpecWorks.JsonDiff;
using System.Text.Json.Nodes;

// Parse JSON documents
var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":31,\"city\":\"NYC\"}");

// Generate RFC 6902 patch
var generator = new JsonDiffGenerator();
JsonPatchDocument patch = generator.CreateDiff(source, target);

// Output: RFC 6902 compliant operations
// [
//   { "op": "replace", "path": "/age", "value": 31 },
//   { "op": "add", "path": "/city", "value": "NYC" }
// ]

// Apply patch using Microsoft.AspNetCore.JsonPatch
patch.ApplyTo(source);
// source now equals target
```

**See the [.NET README](dotnet/README.md) for detailed documentation.**

### Python (Future)

```python
from specworks.jsondiff import JsonDiffGenerator
import json

# Parse JSON documents
source = {"name": "Alice", "age": 30}
target = {"name": "Alice", "age": 31, "city": "NYC"}

# Generate RFC 6902 patch
generator = JsonDiffGenerator()
patch = generator.create_diff(source, target)

# Output: RFC 6902 compliant operations
# [
#   {"op": "replace", "path": "/age", "value": 31},
#   {"op": "add", "path": "/city", "value": "NYC"}
# ]
```

### Rust (Future)

```rust
use specworks_jsondiff::{JsonDiffGenerator, JsonDiffGeneratorImpl};
use serde_json::json;

// Parse JSON documents
let source = json!({"name": "Alice", "age": 30});
let target = json!({"name": "Alice", "age": 31, "city": "NYC"});

// Generate RFC 6902 patch
let generator = JsonDiffGeneratorImpl::new();
let patch = generator.create_diff(&source, &target);

// Output: RFC 6902 compliant operations
// [
//   PatchOperation::Replace { path: "/age".into(), value: json!(31) },
//   PatchOperation::Add { path: "/city".into(), value: json!("NYC") }
// ]
```

## Architecture Decision Records

The design of this component is documented in ADRs:

1. **[ADR 0001: RFC 6902 Only - No Format Options](adr/0001-rfc-6902-only.md)**
   - Why we support only RFC 6902
   - Why we reject proprietary formats
   - How this ensures interoperability

2. **[ADR 0002: Wrapper Architecture with External Library](adr/0002-wrapper-architecture.md)**
   - Why we use a wrapper pattern
   - Temporary use of external libraries
   - Migration path to native implementation

## Project Structure

```
JsonDiff/
├── adr/                        # Architecture Decision Records
│   ├── 0001-rfc-6902-only.md
│   ├── 0002-wrapper-architecture.md
│   └── 0003-multi-language-implementation.md
├── dotnet/                     # .NET implementation (v1.0 ✅)
│   ├── src/SpecWorks.JsonDiff/
│   │   ├── IJsonDiffGenerator.cs
│   │   ├── JsonDiffGenerator.cs
│   │   ├── Adapters/
│   │   │   └── SystemTextJsonAdapter.cs
│   │   └── SpecWorks.JsonDiff.csproj
│   ├── tests/SpecWorks.JsonDiff.Tests/
│   │   ├── JsonDiffGeneratorTests.cs
│   │   ├── JsonPatchTestSuiteTests.cs
│   │   └── SpecWorks.JsonDiff.Tests.csproj
│   ├── SpecWorks.JsonDiff.sln
│   └── README.md
├── python/                     # Python implementation (planned)
│   ├── src/specworks/jsondiff/
│   ├── tests/
│   └── README.md
├── rust/                       # Rust implementation (planned)
│   ├── src/
│   ├── Cargo.toml
│   └── README.md
├── testcases/                  # Shared test fixtures
│   ├── rfc-examples.json
│   ├── edge-cases.json
│   └── expected-outputs.json
├── specs.json                  # Specification linkset
└── README.md                   # This file
```

## RFC 6902 Operations

JSON Patch supports six operation types:

| Operation | Description | Example |
|-----------|-------------|---------|
| `add` | Add a value to an object or array | `{"op":"add","path":"/name","value":"Alice"}` |
| `remove` | Remove a value from an object or array | `{"op":"remove","path":"/name"}` |
| `replace` | Replace a value | `{"op":"replace","path":"/age","value":31}` |
| `move` | Move a value from one location to another | `{"op":"move","from":"/old","path":"/new"}` |
| `copy` | Copy a value from one location to another | `{"op":"copy","from":"/orig","path":"/copy"}` |
| `test` | Test that a value at path equals specified value | `{"op":"test","path":"/name","value":"Alice"}` |

## What This Component Does NOT Support

In accordance with [ADR 0001](adr/0001-rfc-6902-only.md), this component intentionally does NOT support:

- ❌ **RFC 7386 (JSON Merge Patch)** - Different specification
- ❌ **Proprietary diff formats** - Vendor-specific formats
- ❌ **Non-standard extensions** - Custom operations
- ❌ **Format configuration options** - RFC 6902 only, always

This constraint ensures:
- Interoperability across systems
- Standards compliance
- Predictable behavior
- Future migration safety

## Implementation Status

### Current Phase: .NET v1.0 Released
- ✅ Architecture Decision Records complete
- ✅ Design patterns established
- ✅ Multi-language strategy defined
- ✅ .NET v1.0 implementation complete
- ✅ Comprehensive test suite (55+ tests)
- ✅ NuGet package ready
- ⏳ Python implementation (next)
- ⏳ Rust implementation

## Contributing

This component is part of the SpecWorks project. Contributions are welcome!

Please:
1. Review the [ADRs](adr/) to understand design decisions
2. Follow RFC 6902 strictly
3. Add tests for new functionality
4. Maintain consistency with existing SpecWorks components

## License

MIT License - Available for educational and commercial use.

## References

- [RFC 6902 - JavaScript Object Notation (JSON) Patch](https://datatracker.ietf.org/doc/html/rfc6902)
- [JSON Patch Website](https://jsonpatch.com/)
- [SpecWorks Vision](../VISION.md)
- [vCard Component](../vCard/) - Example multi-language SpecWorks component

## Related Projects

This JSON Patch generator is part of the SpecWorks collection of specification-based software components. See the [specification folder](../specification) for the xRegistry catalog.
