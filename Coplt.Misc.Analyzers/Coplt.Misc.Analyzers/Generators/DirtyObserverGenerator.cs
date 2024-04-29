using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Coplt.Miscellaneous.Analysis.Generators.Templates;
using Coplt.Miscellaneous.Analysis.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Coplt.Misc.Miscellaneous.Analyzers.Generators;

[Generator]
public class DirtyObserverGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var sources = context.SyntaxProvider.ForAttributeWithMetadataName(
                "Coplt.Miscellaneous.DirtyObserverAttribute",
                (syntax, _) => syntax is StructDeclarationSyntax or ClassDeclarationSyntax or RecordDeclarationSyntax,
                static (ctx, _) =>
                {
                    var diagnostics = new List<Diagnostic>();
                    var syntax = (TypeDeclarationSyntax)ctx.TargetNode;
                    var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
                    var rawFullName = symbol.ToDisplayString();
                    var nameWraps = symbol.WrapNames();
                    var nameWrap = symbol.WrapName();

                    var sources = symbol.GetMembers()
                        .Where(m => m is
                            IFieldSymbol
                            {
                                CanBeReferencedByName: true,
                            }
                            or IPropertySymbol
                            {
                                CanBeReferencedByName: true,
                            })
                        .Select(m =>
                        {
                            var is_source = m.GetAttributes().Any(a =>
                                a.AttributeClass?.ToDisplayString() == "Coplt.Miscellaneous.DirtySourceAttribute");
                            return is_source ? m.Name : null;
                        })
                        .Where(n => n != null)
                        .Cast<string>()
                        .ToImmutableArray();

                    return (
                        syntax, rawFullName, nameWraps, nameWrap,
                        sources,
                        diagnostics: AlwaysEq.Create(diagnostics)
                    );
                }
            )
            .Select(static (input, _) =>
            {
                var (syntax, rawFullName, nameWraps, nameWrap,
                    sources,
                    diagnostics) = input;

                var nullable = NullableContextOptions.Enable;
                var usings = new HashSet<string>();
                Utils.GetUsings(syntax, usings);
                var genBase = new GenBase(rawFullName, nullable, usings, nameWraps, nameWrap);

                var name = syntax.Identifier.ToString();

                return (
                    genBase, name,
                    sources,
                    diagnostics
                );
            });

        context.RegisterSourceOutput(sources, static (ctx, input) =>
        {
            var (genBase, name,
                sources,
                diagnostics) = input;

            if (diagnostics.Value.Count > 0)
            {
                foreach (var diagnostic in diagnostics.Value)
                {
                    ctx.ReportDiagnostic(diagnostic);
                }
            }
            var code = new DirtyObserverTemplate(
                genBase, name, sources
            ).Gen();
            var sourceText = SourceText.From(code, Encoding.UTF8);
            var rawSourceFileName = genBase.FileFullName;
            var sourceFileName = $"{rawSourceFileName}.DirtySource.g.cs";
            ctx.AddSource(sourceFileName, sourceText);
        });
    }
}
