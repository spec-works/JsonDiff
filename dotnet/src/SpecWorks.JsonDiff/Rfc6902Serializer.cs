using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace SpecWorks.JsonDiff;

/// <summary>
/// Provides RFC 6902 compliant serialization for JsonPatchDocument operations.
/// </summary>
/// <remarks>
/// This class ensures that only RFC 6902 specified fields are included in the serialized output,
/// excluding internal fields like OperationType that are not part of the specification.
/// See https://datatracker.ietf.org/doc/html/rfc6902 for the specification.
/// </remarks>
public static class Rfc6902Serializer
{
    /// <summary>
    /// Serializes a JsonPatchDocument to an RFC 6902 compliant JSON string.
    /// </summary>
    /// <param name="patch">The patch document to serialize.</param>
    /// <param name="writeIndented">Whether to format the output with indentation.</param>
    /// <returns>An RFC 6902 compliant JSON string.</returns>
    /// <remarks>
    /// RFC 6902 defines the following fields for each operation type:
    /// - add: op, path, value
    /// - remove: op, path
    /// - replace: op, path, value
    /// - move: op, from, path
    /// - copy: op, from, path
    /// - test: op, path, value
    /// 
    /// This method excludes non-standard fields like OperationType and unnecessary null fields.
    /// </remarks>
    public static string Serialize(JsonPatchDocument patch, bool writeIndented = false)
    {
        if (patch == null)
            throw new ArgumentNullException(nameof(patch));

        var operations = patch.Operations.Select(op => CreateRfc6902Operation(op)).ToList();

        return JsonSerializer.Serialize(operations, new JsonSerializerOptions
        {
            WriteIndented = writeIndented,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Creates an RFC 6902 compliant representation of an operation.
    /// </summary>
    private static object CreateRfc6902Operation(Operation operation)
    {
        var op = operation.op.ToLowerInvariant();

        // RFC 6902 Section 4: Operations
        // Each operation MUST have exactly the fields specified for that operation type
        switch (op)
        {
            case "add":
                // RFC 6902 Section 4.1: add
                // The "add" operation performs one of the following functions:
                // - Adds a new value at the target location
                // Required fields: op, path, value
                return new
                {
                    op = operation.op,
                    path = operation.path,
                    value = operation.value
                };

            case "remove":
                // RFC 6902 Section 4.2: remove
                // The "remove" operation removes the value at the target location.
                // Required fields: op, path
                return new
                {
                    op = operation.op,
                    path = operation.path
                };

            case "replace":
                // RFC 6902 Section 4.3: replace
                // The "replace" operation replaces the value at the target location.
                // Required fields: op, path, value
                return new
                {
                    op = operation.op,
                    path = operation.path,
                    value = operation.value
                };

            case "move":
                // RFC 6902 Section 4.4: move
                // The "move" operation removes the value at "from" and adds it to "path".
                // Required fields: op, from, path
                return new
                {
                    op = operation.op,
                    from = operation.from,
                    path = operation.path
                };

            case "copy":
                // RFC 6902 Section 4.5: copy
                // The "copy" operation copies the value at "from" to "path".
                // Required fields: op, from, path
                return new
                {
                    op = operation.op,
                    from = operation.from,
                    path = operation.path
                };

            case "test":
                // RFC 6902 Section 4.6: test
                // The "test" operation tests that a value at the target location is equal to a specified value.
                // Required fields: op, path, value
                return new
                {
                    op = operation.op,
                    path = operation.path,
                    value = operation.value
                };

            default:
                throw new InvalidOperationException($"Unknown operation type: {op}");
        }
    }
}
