# JSON Diff CLI

Command-line tool for generating RFC 6902 JSON Patch documents by comparing two JSON files.

## Installation

### As a .NET Global Tool (Recommended)

Install from NuGet:

```bash
dotnet tool install --global SpecWorks.JsonDiff.Cli
```

Update to the latest version:

```bash
dotnet tool update --global SpecWorks.JsonDiff.Cli
```

Uninstall:

```bash
dotnet tool uninstall --global SpecWorks.JsonDiff.Cli
```

### From Source

Build and pack locally:

```bash
cd src/SpecWorks.JsonDiff.Cli
dotnet pack
dotnet tool install --global --add-source ./bin/Debug SpecWorks.JsonDiff.Cli
```

## Usage

```bash
jsondiff <source-file> <target-file> [options]
```

### Arguments

- `<source-file>` - Path to the source JSON file
- `<target-file>` - Path to the target JSON file

### Options

- `--pretty` - Format output with indentation
- `--allow-comments` - Allow comments in JSON input files (JSONC format)
- `--help` - Show help message

### Examples

Compare two JSON files:

```bash
dotnet run -- source.json target.json
```

Compare with pretty-printed output:

```bash
dotnet run -- source.json target.json --pretty
```

Pipe output to a file:

```bash
dotnet run -- old.json new.json --pretty > patch.json
```

Compare JSON files with comments (JSONC):

```bash
dotnet run -- config-old.jsonc config-new.jsonc --allow-comments --pretty
```

## Output

The tool outputs a JSON Patch document (RFC 6902) to stdout. The patch represents the operations needed to transform the source JSON into the target JSON.

Example output:

```json
[
  { "op": "replace", "path": "/age", "value": 31 },
  { "op": "remove", "path": "/city" },
  { "op": "add", "path": "/country", "value": "USA" }
]
```

## JSONC Support

The CLI supports JSON files with comments (JSONC format) when using the `--allow-comments` option. This is useful for configuration files and other JSON documents that include documentation comments.

Supported comment styles:
- Single-line comments: `// comment`
- Multi-line comments: `/* comment */`

Example JSONC file:

```jsonc
{
  // User information
  "name": "Alice",
  "age": 30,  // Updated annually
  /* Contact details */
  "city": "Boston"
}
```

**Note:** The `--allow-comments` option is opt-in to maintain strict RFC 6902 compliance by default. Regular JSON files work with or without this option.

## Exit Codes

- `0` - Success
- `1` - Invalid arguments or file not found
- `2` - Invalid JSON in input files
- `3` - Error generating diff

## RFC 6902 Compliance

This tool generates JSON Patch documents that strictly comply with [RFC 6902](https://datatracker.ietf.org/doc/html/rfc6902). It supports all six operation types:

- `add` - Add a value
- `remove` - Remove a value
- `replace` - Replace a value
- `move` - Move a value
- `copy` - Copy a value
- `test` - Test that a value equals specified

## Examples

### Basic Usage

```bash
# Compare two files
dotnet run -- before.json after.json

# Pretty-printed output
dotnet run -- before.json after.json --pretty
```

### Real-World Scenarios

```bash
# Generate patch for configuration changes
dotnet run -- config-v1.json config-v2.json --pretty > config.patch

# Compare API responses
dotnet run -- response-old.json response-new.json

# Version control integration
dotnet run -- committed.json working.json --pretty
```

## Integration with Other Tools

The output can be used with any RFC 6902 compliant JSON Patch implementation:

```bash
# Generate patch
dotnet run -- old.json new.json > changes.patch

# Apply patch using another tool
json-patch apply old.json changes.patch > result.json
```

## Related Projects

This CLI tool uses the [SpecWorks.JsonDiff](../SpecWorks.JsonDiff) library, which is part of the [SpecWorks](../../../../) collection of specification-based software components.

## License

MIT License - See LICENSE file for details.
