using SpecWorks.JsonDiff;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SpecWorks.JsonDiff.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Contains("--help"))
        {
            Console.WriteLine("JSON Diff - RFC 6902 JSON Patch Generator");
            Console.WriteLine();
            Console.WriteLine("Generates RFC 6902 compliant JSON Patch documents by comparing two JSON files.");
            Console.WriteLine();
            Console.WriteLine("Usage: jsondiff <source-file> <target-file> [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <source-file>  Path to the source JSON file");
            Console.WriteLine("  <target-file>  Path to the target JSON file");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --pretty       Format output with indentation");
            Console.WriteLine("  --help         Show this help message");
            Console.WriteLine();
            Console.WriteLine("Output:");
            Console.WriteLine("  The tool outputs a JSON Patch document (RFC 6902) to stdout.");
            Console.WriteLine("  The patch can be applied to the source to transform it into the target.");
            Console.WriteLine();
            Console.WriteLine("Exit Codes:");
            Console.WriteLine("  0  Success");
            Console.WriteLine("  1  Invalid arguments or file not found");
            Console.WriteLine("  2  Invalid JSON in input files");
            Console.WriteLine("  3  Error generating diff");
            return 0;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: jsondiff <source-file> <target-file> [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  <source-file>  Path to the source JSON file");
            Console.WriteLine("  <target-file>  Path to the target JSON file");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --pretty       Format output with indentation");
            Console.WriteLine("  --help         Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  jsondiff old.json new.json");
            Console.WriteLine("  jsondiff source.json target.json --pretty");
            return 1;
        }

        string sourceFile = args[0];
        string targetFile = args[1];
        bool pretty = args.Contains("--pretty");

        try
        {
            // Check if files exist
            if (!File.Exists(sourceFile))
            {
                Console.Error.WriteLine($"Error: Source file '{sourceFile}' not found.");
                return 1;
            }

            if (!File.Exists(targetFile))
            {
                Console.Error.WriteLine($"Error: Target file '{targetFile}' not found.");
                return 1;
            }

            // Read and parse JSON files
            JsonNode? source;
            JsonNode? target;

            try
            {
                string sourceContent = await File.ReadAllTextAsync(sourceFile);
                source = JsonNode.Parse(sourceContent);

                if (source == null)
                {
                    Console.Error.WriteLine($"Error: Source file '{sourceFile}' contains invalid or null JSON.");
                    return 2;
                }
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Error: Failed to parse source file '{sourceFile}': {ex.Message}");
                return 2;
            }

            try
            {
                string targetContent = await File.ReadAllTextAsync(targetFile);
                target = JsonNode.Parse(targetContent);

                if (target == null)
                {
                    Console.Error.WriteLine($"Error: Target file '{targetFile}' contains invalid or null JSON.");
                    return 2;
                }
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Error: Failed to parse target file '{targetFile}': {ex.Message}");
                return 2;
            }

            // Generate diff
            var generator = new JsonDiffGenerator();
            var patch = generator.CreateDiff(source, target);

            // Convert patch to RFC 6902 compliant JSON
            string patchJson = Rfc6902Serializer.Serialize(patch, writeIndented: pretty);

            // Output result
            Console.WriteLine(patchJson);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 3;
        }
    }
}
