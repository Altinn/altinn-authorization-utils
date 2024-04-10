using Altinn.Urn.SourceGenerator.Emitting;
using Altinn.Urn.SourceGenerator.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Altinn.Urn.SourceGenerator;

[Generator]
public class UrnGenerator
    : IIncrementalGenerator
{
    [ThreadStatic]
    private static StringBuilder? _fileNameBuilder;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var records = context.SyntaxProvider.ForAttributeWithMetadataName(
            TypeSymbols.MetadataNames.KeyValueUrnAttribute,
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

            var fileNameBuilder = _fileNameBuilder ??= new StringBuilder();
            fileNameBuilder.Clear();

            fileNameBuilder.Append(input.RecordInfo.Namespace);
            fileNameBuilder.Append('.');
            foreach (var outerType in input.RecordInfo.ContainingTypes)
            {
                fileNameBuilder.Append(outerType.Name);
                fileNameBuilder.Append('.');
            }

            fileNameBuilder.Append(input.RecordInfo.TypeName);
            fileNameBuilder.Append(".g.cs");

            var result = UrnRecordEmitter.Emit(input.RecordInfo, input.JsonConverterAttribute, input.JsonConverterConcreteType, ctx.CancellationToken);
            ctx.AddSource(fileNameBuilder.ToString(), result);
        });
    }
}
