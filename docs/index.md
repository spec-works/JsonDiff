# JsonDiff Documentation

Generate JSON Patch documents by comparing two JSON objects according to [RFC 6902](https://www.rfc-editor.org/rfc/rfc6902).

## What is JsonDiff?

JsonDiff is a .NET library that generates [JSON Patch](https://www.rfc-editor.org/rfc/rfc6902) documents by comparing two JSON objects. It produces the minimal set of operations needed to transform one JSON document into another.

## Installation

### Library

Install via NuGet:

```bash
dotnet add package SpecWorks.JsonDiff
```

### CLI Tool

Install the command-line tool globally:

```bash
dotnet tool install --global SpecWorks.JsonDiff.Cli
```

## Features

- ✅ **RFC 6902 Compliant** - Generates standard JSON Patch documents
- ✅ **Minimal Diffs** - Produces the smallest set of operations
- ✅ **Array Handling** - Smart array diff algorithms
- ✅ **CLI Tool** - Command-line interface for quick diffs
- ✅ **Type-Safe API** - Strong typing with nullable reference types
- ✅ **Comprehensive Testing** - 20+ tests covering specification requirements
- ✅ **Multi-Target** - Supports .NET 10.0 and .NET 8.0 (LTS)

## Quick Start

### Library Usage

```csharp
using System.Text.Json;
using SpecWorks.JsonDiff;

// Parse JSON documents
var original = JsonDocument.Parse("{\"name\":\"John\",\"age\":30}");
var modified = JsonDocument.Parse("{\"name\":\"Jane\",\"age\":30}");

// Generate diff
var patch = JsonDiffer.Diff(original, modified);

// Output: [{"op":"replace","path":"/name","value":"Jane"}]
Console.WriteLine(patch);
```

### CLI Usage

```bash
# Compare two JSON files
jsondiff original.json modified.json

# Output to file
jsondiff original.json modified.json -o patch.json

# Compare JSON strings
jsondiff --string '{"a":1}' '{"a":2}'
```

## Use Cases

### Version Control for Configuration

Generate diffs between configuration file versions:

```csharp
var oldConfig = JsonDocument.Parse(File.ReadAllText("config.v1.json"));
var newConfig = JsonDocument.Parse(File.ReadAllText("config.v2.json"));
var changes = JsonDiffer.Diff(oldConfig, newConfig);
```

### API Response Comparison

Compare API responses for testing:

```csharp
var expected = JsonDocument.Parse(expectedJson);
var actual = JsonDocument.Parse(actualJson);
var diff = JsonDiffer.Diff(expected, actual);

if (diff.RootElement.GetArrayLength() > 0)
{
    Console.WriteLine("Responses differ:");
    Console.WriteLine(diff);
}
```

### Audit Logging

Track changes to JSON documents:

```csharp
var before = JsonDocument.Parse(beforeJson);
var after = JsonDocument.Parse(afterJson);
var auditLog = JsonDiffer.Diff(before, after);

// Store audit log with timestamp
SaveAuditLog(timestamp, auditLog);
```

## API Reference

- [API Documentation](api/SpecWorks.JsonDiff.html) - Complete API reference

## Specification Compliance

This library implements [RFC 6902 - JavaScript Object Notation (JSON) Patch](https://www.rfc-editor.org/rfc/rfc6902).

### Supported Operations

| Operation | RFC Section | Status |
|-----------|-------------|--------|
| add | 4.1 | ✅ Supported |
| remove | 4.2 | ✅ Supported |
| replace | 4.3 | ✅ Supported |
| move | 4.4 | ✅ Supported |
| copy | 4.5 | ✅ Supported |
| test | 4.6 | ✅ Supported |

## CLI Reference

### Commands

```bash
jsondiff <original> <modified> [options]
```

### Options

- `-o, --output <file>` - Output file path (default: stdout)
- `-s, --string` - Treat arguments as JSON strings instead of files
- `-p, --pretty` - Pretty-print the output
- `-h, --help` - Show help information
- `--version` - Show version information

### Examples

```bash
# Compare files
jsondiff old.json new.json

# Compare with pretty output
jsondiff old.json new.json --pretty

# Compare JSON strings
jsondiff -s '{"a":1}' '{"a":2}'

# Save to file
jsondiff old.json new.json -o changes.patch
```

## Requirements

- .NET 10.0 or .NET 8.0 (LTS)
- C# 10.0 or later

## Source Code

View the source code on [GitHub](https://github.com/spec-works/JsonDiff).

## Contributing

Contributions welcome! See the [repository](https://github.com/spec-works/JsonDiff) for:
- Issue tracking
- Pull request guidelines
- Architecture Decision Records (ADRs)

## License

MIT License - see [LICENSE](https://github.com/spec-works/JsonDiff/blob/main/LICENSE) for details.

## Links

- **GitHub Repository**: [github.com/spec-works/JsonDiff](https://github.com/spec-works/JsonDiff)
- **RFC 6902 Specification**: [rfc-editor.org/rfc/rfc6902](https://www.rfc-editor.org/rfc/rfc6902)
- **SpecWorks Factory**: [spec-works.github.io](https://spec-works.github.io)
