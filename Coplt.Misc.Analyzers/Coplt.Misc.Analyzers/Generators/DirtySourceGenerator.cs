using System.Collections.Generic;
using System.Linq;
using System.Text;
using Coplt.Miscellaneous.Analysis.Generators.Templates;
using Coplt.Miscellaneous.Analysis.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coplt.Misc.Miscellaneous.Analyzers.Generators;

[Generator]
public class DirtySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Coplt.Miscellaneous.DirtySourceAttribute",
                (syntax, _) => syntax is StructDeclarationSyntax or ClassDeclarationSyntax or RecordDeclarationSyntax,
                static (ctx, _) =>
                {
                    var diagnostics = new List<Diagnostic>();
                    var syntax = (TypeDeclarationSyntax)ctx.TargetNode;
                    var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                    var rawFullName = symbol.ToDisplayString();
                    var nameWraps = symbol.WrapNames();
                    var nameWrap = symbol.WrapName();

                    return (
                        syntax, rawFullName, nameWraps, nameWrap,
                        diagnostics: AlwaysEq.Create(diagnostics)
                    );
                }
            )
            .Select(static (input, _) =>
            {
                var (syntax, rawFullName, nameWraps, nameWrap,
                    diagnostics) = input;

                var nullable = NullableContextOptions.Enable;
                var usings = new HashSet<string>();
                Utils.GetUsings(syntax, usings);
                var genBase = new GenBase(rawFullName, nullable, usings, nameWraps, nameWrap);

                var name = syntax.Identifier.ToString();

                return (
                    genBase, name,
                    diagnostics
                );
            });

        context.RegisterSourceOutput(sources, static (ctx, input) =>
        {
            var (genBase, name,
                diagnostics) = input;

            if (diagnostics.Value.Count > 0)
            {
                foreach (var diagnostic in diagnostics.Value)
                {
                    ctx.ReportDiagnostic(diagnostic);
                }
            }
            var code = new DirtySourceTemplate(
                genBase, name
            ).Gen();
            var sourceText = SourceText.From(code, Encoding.UTF8);
            var rawSourceFileName = genBase.FileFullName;
            var sourceFileName = $"{rawSourceFileName}.DirtySource.g.cs";
            ctx.AddSource(sourceFileName, sourceText);
        });
    }
}
