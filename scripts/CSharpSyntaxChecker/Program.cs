using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var root = args.Length > 0 ? args[0] : "disputatio/Assets";
var basePath = Path.GetFullPath(root);
if (!Directory.Exists(basePath))
{
    Console.Error.WriteLine($"Directory not found: {basePath}");
    Environment.Exit(2);
}

var errorCount = 0;
foreach (var path in Directory.EnumerateFiles(basePath, "*.cs", SearchOption.AllDirectories))
{
    string text;
    try
    {
        text = File.ReadAllText(path);
    }
    catch (IOException ex)
    {
        Console.WriteLine($"ERROR|{path}|Failed to read file: {ex.Message}");
        errorCount++;
        continue;
    }

    var tree = CSharpSyntaxTree.ParseText(text, path: path);
    foreach (var d in tree.GetDiagnostics())
    {
        if (d.Severity != DiagnosticSeverity.Error)
            continue;
        var msg = d.GetMessage();
        Console.WriteLine($"ERROR|{path}|{d.Id}: {msg}");
        errorCount++;
    }
}

Environment.Exit(errorCount > 0 ? 1 : 0);
