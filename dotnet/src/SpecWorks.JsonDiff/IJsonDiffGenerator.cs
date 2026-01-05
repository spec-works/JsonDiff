using System.Text.Json.Nodes;
using Microsoft.AspNetCore.JsonPatch;

namespace SpecWorks.JsonDiff;

/// <summary>
/// Generates RFC 6902 compliant JSON Patch documents by comparing two JSON documents.
/// </summary>
/// <remarks>
/// This interface strictly implements RFC 6902 (JSON Patch) and does not support
/// other formats such as JSON Merge Patch (RFC 7386) or proprietary formats.
/// This constraint ensures interoperability and standards compliance.
/// See https://datatracker.ietf.org/doc/html/rfc6902 for the specification.
/// </remarks>
public interface IJsonDiffGenerator
{
    /// <summary>
    /// Creates an RFC 6902 JSON Patch document representing the differences
    /// between the source and target JSON documents.
    /// </summary>
    /// <param name="source">The source JSON document to compare from.</param>
    /// <param name="target">The target JSON document to compare to.</param>
    /// <returns>
    /// A <see cref="JsonPatchDocument"/> containing RFC 6902 operations (add, remove,
    /// replace, move, copy, test) that transform the source into the target.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> or <paramref name="target"/> is null.
    /// </exception>
    /// <remarks>
    /// The returned patch document can be applied to the source document using
    /// standard JSON Patch implementations to produce the target document.
    ///
    /// RFC 6902 Operations:
    /// - add: Add a value to an object or array
    /// - remove: Remove a value from an object or array
    /// - replace: Replace a value
    /// - move: Move a value from one location to another
    /// - copy: Copy a value from one location to another
    /// - test: Test that a value at path equals specified value
    /// </remarks>
    /// <example>
    /// <code>
    /// var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
    /// var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":31,\"city\":\"NYC\"}");
    ///
    /// var generator = new JsonDiffGenerator();
    /// var patch = generator.CreateDiff(source, target);
    ///
    /// // Result: [
    /// //   { "op": "replace", "path": "/age", "value": 31 },
    /// //   { "op": "add", "path": "/city", "value": "NYC" }
    /// // ]
    /// </code>
    /// </example>
    JsonPatchDocument CreateDiff(JsonNode source, JsonNode target);
}
