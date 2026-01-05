using System.Text.Json.Nodes;
using Microsoft.AspNetCore.JsonPatch;
using SpecWorks.JsonDiff.Adapters;

namespace SpecWorks.JsonDiff;

/// <summary>
/// Default implementation of <see cref="IJsonDiffGenerator"/> that generates
/// RFC 6902 compliant JSON Patch documents.
/// </summary>
/// <remarks>
/// <para>
/// This class serves as a facade that hides the internal implementation details.
/// The current version (v1.x) uses an adapter pattern with SystemTextJson.JsonDiffPatch
/// as the underlying implementation.
/// </para>
/// <para>
/// Future versions (v2.x) will use a native SpecWorks-generated RFC 6902 implementation,
/// but the public API will remain unchanged, ensuring seamless migration for consumers.
/// </para>
/// <para>
/// This component strictly implements RFC 6902 and does not support other formats
/// such as JSON Merge Patch (RFC 7386) or proprietary diff formats. This constraint
/// ensures interoperability and standards compliance.
/// </para>
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// using SpecWorks.JsonDiff;
/// using System.Text.Json.Nodes;
///
/// var source = JsonNode.Parse("{\"name\":\"Alice\",\"age\":30}");
/// var target = JsonNode.Parse("{\"name\":\"Alice\",\"age\":31,\"city\":\"NYC\"}");
///
/// var generator = new JsonDiffGenerator();
/// var patch = generator.CreateDiff(source, target);
///
/// // Apply the patch
/// patch.ApplyTo(source);
/// // source now equals target
/// </code>
/// </example>
public class JsonDiffGenerator : IJsonDiffGenerator
{
    private readonly IJsonDiffGenerator _implementation;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDiffGenerator"/> class.
    /// </summary>
    public JsonDiffGenerator()
    {
        // v1.x: Use adapter for SystemTextJson.JsonDiffPatch
        _implementation = new SystemTextJsonAdapter();

        // v2.x: Will use native implementation
        // _implementation = new JsonDiffGeneratorImpl();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDiffGenerator"/> class
    /// with a custom implementation.
    /// </summary>
    /// <param name="implementation">The underlying implementation to use.</param>
    /// <remarks>
    /// This constructor is primarily for testing and advanced scenarios.
    /// Most consumers should use the parameterless constructor.
    /// </remarks>
    internal JsonDiffGenerator(IJsonDiffGenerator implementation)
    {
        _implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
    }

    /// <inheritdoc />
    public JsonPatchDocument CreateDiff(JsonNode source, JsonNode target)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (target == null) throw new ArgumentNullException(nameof(target));

        return _implementation.CreateDiff(source, target);
    }
}
