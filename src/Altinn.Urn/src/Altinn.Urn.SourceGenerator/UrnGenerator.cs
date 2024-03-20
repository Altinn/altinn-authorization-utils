using Altinn.Urn.SourceGenerator.Emitting;
using Altinn.Urn.SourceGenerator.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Altinn.Urn.SourceGenerator;

[Generator]
public class UrnGenerator
    : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var records = context.SyntaxProvider.ForAttributeWithMetadataName(
            TypeSymbols.MetadataNames.UrnAttribute,
            predicate: static (ctx, _) => ctx is RecordDeclarationSyntax,
            transform: static (ctx, ct) => UrnRecordParser.Parse(ctx, ct))
            .Where(static v => !v.IsDefault);

        // Generate code for each record
        context.RegisterSourceOutput(records, static (ctx, input) =>
        {
            // First, just emit all diagnostics
            foreach (var diagnostic in input.Diagnostics)
            {
                List<Location>? additionalLocations = null;
                if (!diagnostic.AdditionalLocations.IsDefault)
                {
                    var span = diagnostic.AdditionalLocations.AsSpan();
                    if (span.Length > 0)
                    {
                        additionalLocations = new(span.Length);
                        foreach (var location in span)
                        {
                            additionalLocations.Add(location.ToLocation());
                        }
                    }
                }

                ctx.ReportDiagnostic(
                    Diagnostic.Create(
                        diagnostic.Descriptor, 
                        diagnostic.Location?.ToLocation(), 
                        diagnostic.Severity,
                        additionalLocations, 
                        properties: null, 
                        messageArgs: null));
            }

            if (!input.HasRecordInfo)
            {
                return;
            }

            var result = UrnRecordEmitter.Emit(input.RecordInfo, input.JsonConverterAttribute, input.JsonConverterConcreteType, ctx.CancellationToken);
            ctx.AddSource(input.RecordInfo.TypeName + ".g.cs", result);
        });
    }
}
