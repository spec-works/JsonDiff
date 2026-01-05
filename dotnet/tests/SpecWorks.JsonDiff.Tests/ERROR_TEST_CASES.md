# Error Test Cases - Design Decisions and Documentation

## Overview

This document explains the design decisions made when addressing error test cases from the JSON Patch test suite in the context of the JsonDiffGenerator component.

## Background

The JSON Patch test suite (from https://github.com/json-patch/json-patch-tests) contains 36 test cases marked with an `error` field:
- 31 from `tests.json`
- 5 from `spec_tests.json`

These test cases were previously filtered out in `JsonPatchTestSuiteTests.cs` (line 29):
```csharp
.Where(t => !t.Disabled && t.Error == null) // Filter out error and disabled tests
```

## Key Insight: Patch Application vs. Patch Generation

The error test cases in the JSON Patch test suite are designed to validate **patch application** failures, not patch generation. They test scenarios where:

1. A patch document is malformed (missing required fields, invalid JSON Pointer syntax)
2. A patch operation should fail when applied (out of bounds index, nonexistent path)
3. A test operation should fail (value mismatch)

However, **JsonDiffGenerator is a diff generator, not a patch applier**. It:
- Takes two valid JSON documents as input
- Generates a patch to transform the source into the target
- Does not apply patches or validate patch operations

## Design Decision: Adapt Error Scenarios as Edge Case Tests

Rather than trying to directly use error test cases (which test patch application), we created a new test class `JsonPatchErrorEdgeCaseTests` that:

1. **Tests edge cases inspired by error scenarios** - Uses the error scenarios as inspiration for edge cases that the diff generator should handle gracefully
2. **Validates generated patches are well-formed** - Ensures the generator produces valid RFC 6902 patches
3. **Focuses on diff generation, not application** - Tests what the generator does, not what a patch applier should do

## Categories of Error Test Cases and Our Response

### 1. Patch Structure Validation (Not Applicable)
**Examples:**
- Missing 'path' parameter
- Missing 'value' parameter
- Invalid JSON Pointer (not starting with /)
- Duplicate operation fields
- Unrecognized operation types

**Our Response:** Not applicable to diff generator. The generator **creates** patches, it doesn't validate them. These tests are for patch consumers/appliers.

### 2. Array Boundary Cases (Adapted)
**Examples:**
- Out of bounds (upper): `/bar/8` when array has only 2 elements
- Out of bounds (lower): `/bar/-1` (negative index)
- Index greater than array length

**Our Response:** Created tests that verify the diff generator handles:
- Array growth (small to large)
- Array shrinking (large to small)
- Empty array transformations

These tests ensure the generator produces valid operations when arrays change size.

### 3. Type Mismatch Cases (Adapted)
**Examples:**
- Object operation on array target: `/bar` on an array
- Array index on object: `/1e0` on object vs array

**Our Response:** Created tests that verify the generator handles:
- Object-to-array transformations
- Array-to-object transformations
- Properties with number-like names (e.g., "1e0", "00", "01")

### 4. Nonexistent Path Cases (Adapted)
**Examples:**
- Adding to nonexistent parent: `/a/b` when `/a` doesn't exist
- Removing nonexistent field
- Removing deep nonexistent path

**Our Response:** Created tests that verify the generator:
- Creates complete objects (not partial paths) when needed
- Handles deep path additions and removals correctly
- Produces operations that can be successfully applied

### 5. Bad Number Formats (Adapted)
**Examples:**
- Scientific notation in paths: `/1e0`
- Leading zeros in array indices: `/00`, `/01`

**Our Response:** Created tests that verify the generator correctly distinguishes between:
- Object property names that look like numbers
- Actual array indices
- Special property names with leading zeros or scientific notation

### 6. Root Document Cases (Adapted)
**Examples:**
- Replacing the whole document
- Root-level type changes (object ↔ array)

**Our Response:** Created tests that verify the generator handles:
- Complete document replacement
- Root-level type transformations
- Empty document scenarios

## Test Coverage

The new `JsonPatchErrorEdgeCaseTests` class contains 14 tests organized into categories:

1. **Array Boundary Cases** (3 tests)
   - Array growth, shrinking, empty arrays

2. **Type Mismatch Cases** (2 tests)
   - Object-to-array and array-to-object transformations

3. **Special Characters and Edge Cases** (2 tests)
   - Number-like property names
   - Leading zero property names

4. **Nested Path Cases** (2 tests)
   - Deep path additions and removals

5. **Document Root Cases** (3 tests)
   - Root replacement scenarios

6. **Complex Scenarios** (2 tests)
   - Multi-edge-case transformations
   - Serialization validation

## Assumptions and Constraints

1. **Input Documents Are Valid** - We assume both source and target are valid JSON documents
2. **Generator Produces Valid Patches** - All generated patches should be RFC 6902 compliant
3. **No Input Validation** - The generator doesn't validate that inputs meet specific constraints
4. **Focus on What Works** - Tests verify successful generation, not failure modes

## Benefits of This Approach

1. **Comprehensive Edge Case Coverage** - Tests cover scenarios that might not be in the standard test suite
2. **Generator-Focused** - Tests validate what the generator does, not what it doesn't do
3. **Maintainable** - Tests are clear, well-documented, and grouped by category
4. **RFC 6902 Compliant** - All tests ensure generated patches follow the specification

## Future Considerations

1. **Input Validation** - If we later add validation to reject invalid inputs, we could add tests for those scenarios
2. **Performance** - Edge cases could be used for performance testing with large documents
3. **Additional Scenarios** - As new edge cases are discovered, they can be added to this test class

## Conclusion

The error test cases from the JSON Patch test suite served as valuable inspiration for edge case testing, even though they couldn't be directly applied to a diff generator. By adapting these scenarios, we've created comprehensive tests that ensure JsonDiffGenerator handles edge cases gracefully and produces valid patches in all scenarios.

## References

- [JSON Patch Test Suite](https://github.com/json-patch/json-patch-tests)
- [RFC 6902 - JSON Patch](https://datatracker.ietf.org/doc/html/rfc6902)
- [Original Issue: Add tests for error test cases in test suite](#)
