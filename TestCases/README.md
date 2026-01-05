# JSON Patch Test Cases

This directory contains the official JSON Patch test suite from the [json-patch/json-patch-tests](https://github.com/json-patch/json-patch-tests) repository.

## Test Files

### `tests.json`
Comprehensive test suite with 72+ test cases covering:
- Empty patches and no-op changes
- Add, remove, replace, move, and copy operations
- Object and array manipulations
- Nested structures and edge cases
- Path encoding per RFC 6901
- Array operations with `-` notation

### `spec_tests.json`
Test cases derived directly from RFC 6902 specification examples (17 tests).

## Test Format

Each test file is a JSON array containing test records with the following structure:

```json
{
  "comment": "Description of what this test validates",
  "doc": {/* Initial JSON document */},
  "patch": [/* Array of RFC 6902 operations */],
  "expected": {/* Expected result after applying patch */},
  "error": "Optional: description of expected error",
  "disabled": false
}
```

### Required Fields
- `doc`: The initial JSON document
- `patch`: Array of RFC 6902 JSON Patch operations to apply

### Optional Fields
- `expected`: The anticipated result (for successful operations)
- `error`: Description of expected error condition (for invalid patches)
- `comment`: Human-readable test description
- `disabled`: Boolean flag to skip a test

## Usage

These test cases are designed to be language-agnostic and can be used to validate JSON Patch diff generators across multiple implementations.

### Current Usage
- **.NET**: `dotnet/tests/SpecWorks.JsonDiff.Tests/JsonPatchTestSuiteTests.cs`

### Future Implementations
- Python (planned)
- Rust (planned)
- Other languages as needed

## Validation Strategy

When implementing tests for a new language:

1. **Filter Tests**: Skip tests with `"error"` field (these test patch application, not diff generation)
2. **Skip Disabled**: Ignore tests with `"disabled": true`
3. **Generate Diff**: Create a patch from `doc` to `expected`
4. **Apply Patch**: Apply the generated patch to `doc`
5. **Verify Result**: Confirm the result matches `expected`

## License

These test cases are from the json-patch-tests repository. Please see the original repository for licensing information.

## Reference

- RFC 6902: JavaScript Object Notation (JSON) Patch
- RFC 6901: JavaScript Object Notation (JSON) Pointer
- Repository: https://github.com/json-patch/json-patch-tests
