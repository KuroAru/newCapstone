using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace CSharpSyntaxChecker.Tests;

public class SyntaxParseTests
{
    private static IEnumerable<Diagnostic> ParseErrors(string text, string path = "test.cs")
    {
        var tree = CSharpSyntaxTree.ParseText(text, path: path);
        return tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Valid_snippet_has_no_parse_errors()
    {
        const string code = """
            namespace N {
                public class C {
                    public void M() { }
                }
            }
            """;
        Assert.Empty(ParseErrors(code));
    }

    [Fact]
    public void Unclosed_brace_produces_parse_error()
    {
        const string code = "class C { void M() { ";
        Assert.NotEmpty(ParseErrors(code));
    }

    [Fact]
    public void Empty_file_has_no_parse_errors()
    {
        Assert.Empty(ParseErrors(""));
    }
}
