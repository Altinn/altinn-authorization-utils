using Altinn.Urn.SourceGenerator.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Altinn.Urn.SourceGenerator.Parsing;

internal ref struct UrnRecordParser 
{
    internal static UrnRecordParserResult Parse(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        var compilation = ctx.SemanticModel.Compilation;
        if (!TypeSymbols.TryCreate(compilation, ct, out var typeSymbols))
        {
            return default;
        }

        var record = ctx.TargetNode as RecordDeclarationSyntax;
        if (record is null)
        {
            return default;
        }

        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return default;
        }

        using var parser = new UrnRecordParser(in typeSymbols, ctx.SemanticModel, record, typeSymbol);
        parser.ParseUrnRecordCandidate(ct);
        return parser.Finish();
    }

    private readonly TypeSymbols _symbols;
    private readonly SemanticModel _semanticModel;
    private readonly RecordDeclarationSyntax _syntax;
    private readonly INamedTypeSymbol _typeSymbol;
    
    private EquitableArray<DiagnosticInfo>.Builder _diagnostics;
    private EquitableArray<UrnPrefixInfo>.Builder _members;
    private HashSet<string> _seenPrefixes;

    private UrnRecordParser(in TypeSymbols typeSymbols, SemanticModel semanticModel, RecordDeclarationSyntax record, INamedTypeSymbol typeSymbol)
    {
        _symbols = typeSymbols;
        _semanticModel = semanticModel;
        _syntax = record;
        _typeSymbol = typeSymbol;

        _diagnostics = EquitableArray.CreateBuilder<DiagnosticInfo>();
        _members = EquitableArray.CreateBuilder<UrnPrefixInfo>();
        _seenPrefixes = [];
    }

    private UrnRecordParserResult Finish()
    {
        var diagnostics = _diagnostics.ToArray();

        var keyword = "record";
        var @namespace = _typeSymbol.ContainingNamespace.ToDisplayString();
        var containingTypes = GetContainingTypes();
        var typeName = _typeSymbol.Name;
        var jsonConverterAttribute = _symbols.JsonConverterAttribute.ToDisplayString();
        var jsonConverterConcreteType = _symbols.UrnJsonConverterOfT.Construct(_typeSymbol).ToDisplayString();
        var jsonVariantConverterConcreteType = _symbols.UrnVariantJsonConverter.Construct(_typeSymbol, _typeSymbol).ToDisplayString();
        jsonVariantConverterConcreteType = $"{jsonVariantConverterConcreteType[..^1]}.Type>";
        var members = _members.ToArray();

        return new UrnRecordParserResult
        {
            Diagnostics = diagnostics,
            JsonConverterAttribute = jsonConverterAttribute,
            JsonConverterConcreteType = jsonConverterConcreteType,
            JsonVariantConverterConcreteType = jsonVariantConverterConcreteType,
            RecordInfo = new UrnRecordInfo
            {
                Keyword = keyword,
                Namespace = @namespace,
                ContainingTypes = containingTypes,
                TypeName = typeName,
                Members = members,
            },
        };
    }

    public void Dispose()
    {
        _diagnostics.Dispose();
        _members.Dispose();
    }

    private readonly EquitableArray<ContainingType> GetContainingTypes()
    {
        var symbol = _typeSymbol;
        using var containingTypes = EquitableArray.CreateBuilder<ContainingType>();
        while (symbol.ContainingSymbol is INamedTypeSymbol containingType)
        {
            var keyword = symbol.TypeKind switch
            {
                TypeKind.Class => "class",
                TypeKind.Struct or TypeKind.Structure => "struct",
                TypeKind.Interface => "interface",
                _ => "class",
            };

            symbol = containingType;
            containingTypes.Add(new(keyword, symbol.Name));
        }

        return containingTypes.ToArray();
    }

    private void ParseUrnRecordCandidate(CancellationToken ct)
    {
        foreach (var member in _syntax.Members)
        {
            ct.ThrowIfCancellationRequested();

            if (member is not MethodDeclarationSyntax method)
            {
                continue;
            }

            ParseMethodCandidate(method, ct);
        }

        if (_members.Length == 0)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnRecordHasNoUrnTypeMethods, _syntax.GetLocation()));
        }

        if (_typeSymbol.IsValueType)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnRecordIsValueType, _syntax.GetLocation()));
        }

        if (!_typeSymbol.IsAbstract)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnRecordMustBeAbstract, _syntax.GetLocation()));
        }
    }

    private void AddDiagnostic(DiagnosticInfo diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }

    private void ParseMethodCandidate(MethodDeclarationSyntax method, CancellationToken ct)
    {
        foreach (var attributeList in method.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeCtorSymbolInfo = _semanticModel.GetSymbolInfo(attribute, ct);
                var attributeCtorSymbol = attributeCtorSymbolInfo.Symbol as IMethodSymbol;
                if (attributeCtorSymbol is null || !attributeCtorSymbol.ContainingType.Equals(_symbols.UrnKeyAttribute, SymbolEqualityComparer.Default))
                {
                    // badly formed attribute definition, or not the right attribute
                    continue;
                }

                ParseUrnTypeMethod(method, ct);
                return;
            }
        }
    }

    private void ParseUrnTypeMethod(MethodDeclarationSyntax method, CancellationToken ct)
    {
        var methodSymbol = _semanticModel.GetDeclaredSymbol(method, ct);
        if (methodSymbol is null)
        {
            // Something is wrong with the compilation
            return;
        }

        var boundAttributes = methodSymbol.GetAttributes();

        if (boundAttributes.Length == 0)
        {
            // no attributes found
            return;
        }

        var multipleCanonical = false;
        int? canonicalIndex = null;
        using var typePrefixes = EquitableArray.CreateBuilder<string>();
        using var canonicalAttributeLocations = EquitableArray.CreateBuilder<LocationInfo>();

        //foreach (var attributeData in boundAttributes)
        for (var i = 0; i < boundAttributes.Length; i++)
        {
            var attributeData = boundAttributes[i];

            if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, _symbols.UrnKeyAttribute))
            {
                continue;
            }

            // UrnTypeAttribute requires a single string argument
            if (attributeData.ConstructorArguments.Length != 1)
            {
                // error handled by the compiler
                continue;
            }

            var prefixStringArgument = attributeData.ConstructorArguments[0];
            if (prefixStringArgument.Kind == TypedConstantKind.Error)
            {
                // error handled by the compiler
                break;
            }

            if (prefixStringArgument.Type!.SpecialType != SpecialType.System_String)
            {
                // error handled by the compiler
                break;
            }

            if (prefixStringArgument.Value is not string prefix)
            {
                // error handled by the compiler
                break;
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodPrefixIsEmpty, LocationInfo.CreateFrom(attributeData.ApplicationSyntaxReference, ct)));
                continue;
            }

            var trimmed = prefix.Trim();
            if (trimmed.Length != prefix.Length)
            {
                AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodPrefixHasWhitespace, LocationInfo.CreateFrom(attributeData.ApplicationSyntaxReference, ct)));
                prefix = trimmed;
            }

            if (prefix.StartsWith("urn:"))
            {
                AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodPrefixStartsWithUrn, LocationInfo.CreateFrom(attributeData.ApplicationSyntaxReference, ct)));
                prefix = prefix[4..];
            }

            if (prefix.EndsWith(":"))
            {
                AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodPrefixEndsWithColon, LocationInfo.CreateFrom(attributeData.ApplicationSyntaxReference, ct)));
                prefix = prefix.TrimEnd(':');
            }

            if (!_seenPrefixes.Add(prefix))
            {
                AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodPrefixIsDuplicate, LocationInfo.CreateFrom(attributeData.ApplicationSyntaxReference, ct)));
                continue;
            }

            typePrefixes.Add(prefix);

            var canonicalArgument = attributeData.NamedArguments.FirstOrDefault(static namedArg => namedArg.Key == "Canonical");
            if (canonicalArgument.Key is null)
            {
                // canonical not specified - don't do anything with it
                continue;
            }

            if (canonicalArgument.Value.Kind == TypedConstantKind.Error)
            {
                // error handled by the compiler
                continue;
            }

            if (canonicalArgument.Value.Value is not bool isCurrentKeyCanonical)
            {
                // error handled by the compiler
                continue;
            }

            if (!isCurrentKeyCanonical && canonicalIndex is null)
            {
                // explicitly set to false, but no canonical key set yet
                canonicalIndex = -1;
            }

            if (isCurrentKeyCanonical)
            {
                multipleCanonical |= canonicalIndex >= 0;
                var location = LocationInfo.CreateFrom(attributeData.ApplicationSyntaxReference, ct);

                if (location is not null)
                {
                    canonicalAttributeLocations.Add(location);
                }
            }

            if (isCurrentKeyCanonical && canonicalIndex is null or < 0)
            {
                canonicalIndex = i;
            }
        }

        if (typePrefixes.Length == 0)
        {
            // no valid prefixes found
            return;
        }

        // if there is only a single prefix, and no canonical property set, then it is the canonical prefix
        if (canonicalIndex is null && typePrefixes.Length == 1)
        {
            canonicalIndex = 0;
        }

        if (canonicalIndex is null or < 0)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodPrefixIsMissingCanonical, method));
        }

        if (multipleCanonical)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodPrefixHasMultipleCanonical, method, canonicalAttributeLocations.ToArray()));
        }

        if (canonicalIndex is null or < 0)
        {
            // we emit diagnostic errors in this case, so the compile will fail,
            // but we still want to generate code for the prefixes that are valid
            canonicalIndex = 0;
        }

        // This represents a valid-ish urn type method. This should be somewhat lax in what it accepts,
        // such that users will get code completion and diagnostics even when their code contains minor issues.
        var keepMethod = true;

        if (method.Arity > 0)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodIsGeneric, method));
            keepMethod = false;
        }

        var isStatic = false;
        var isPartial = false;
        foreach (var mod in method.Modifiers)
        {
            if (mod.IsKind(SyntaxKind.StaticKeyword))
            {
                isStatic = true;
            }
            else if (mod.IsKind(SyntaxKind.PartialKeyword))
            {
                isPartial = true;
            }
        }

        if (isStatic)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodIsStatic, method));
            keepMethod = false;
        }

        if (!isPartial)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodMustBePartial, method));
            keepMethod = false;
        }

        var methodBody = method.Body as CSharpSyntaxNode ?? method.ExpressionBody;
        if (methodBody is not null)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodHasBody, method));
            keepMethod = false;
        }

        if (!methodSymbol.ReturnType.Equals(_symbols.Bool, SymbolEqualityComparer.Default))
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodMustReturnBool, method));
            keepMethod = false;
        }

        if (methodSymbol.Parameters.Length != 1)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodMustHaveOneParameter, method));
            keepMethod = false;
        }

        var parameter = methodSymbol.Parameters[0];
        if (parameter.RefKind != RefKind.Out)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodParameterMustBeOut, method));
            keepMethod = false;
        }

        var name = methodSymbol.Name;
        string? prefixName = null;
        if (!name.StartsWith("Is", StringComparison.Ordinal))
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodMustStartWithIs, method));
            keepMethod = false;
        }
        else
        {
            prefixName = name.Substring(2);
        }

        var type = parameter.Type;
        if (!keepMethod || type.TypeKind == TypeKind.Error)
        {
            return;
        }

        var tryParseMode = GetTryParseMode(prefixName!, type);
        var formatMode = GetFormatMode(prefixName!, type, ct);

        if (!tryParseMode.IsValid)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnValueMustBeParsable, method));
            keepMethod = false;
        }

        if (!formatMode.IsValid)
        {
            AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnValueMustBeFormattable, method));
            keepMethod = false;
        }

        if (!keepMethod)
        {
            return;
        }

        var prefixInfo = new UrnPrefixInfo
        {
            TryParseMode = tryParseMode,
            FormatMode = formatMode,
            Name = prefixName!,
            Prefixes = typePrefixes.ToArray(),
            CanonicalPrefixIndex = canonicalIndex.Value,
            ValueType = type.ToDisplayString(),
            ValueTypeIsValueType = type.IsValueType,
        };

        _members.Add(prefixInfo);
    }

    private TryParseMode GetTryParseMode(string prefixName, ITypeSymbol argumentType)
    {
        var methodName = $"TryParse{prefixName}";
        foreach (var member in _typeSymbol.GetMembers(methodName))
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.Arity != 0)
            {
                continue;
            }

            if (methodSymbol.Parameters.Length != 3)
            {
                continue;
            }

            var param1 = methodSymbol.Parameters[0];
            var param2 = methodSymbol.Parameters[1];
            var param3 = methodSymbol.Parameters[2];

            if (!param1.Type.Equals(_symbols.ReadOnlySpanOfChar, SymbolEqualityComparer.Default))
            {
                continue;
            }

            if (!param2.Type.Equals(_symbols.IFormatProvider, SymbolEqualityComparer.Default))
            {
                continue;
            }

            if (!param3.Type.Equals(argumentType, SymbolEqualityComparer.Default) || param3.RefKind != RefKind.Out)
            {
                continue;
            }

            // we have a match - check diagnostics later
            return TryParseMode.ExplicitlyDefined;
        }

        if (argumentType.Implements(_symbols.IUrnParsable.Construct(argumentType)))
        {
            return TryParseMode.UrnParsable;
        }

        if (argumentType.Implements(_symbols.ISpanParsable.Construct(argumentType)))
        {
            return TryParseMode.SpanParsable;
        }

        if (argumentType.Implements(_symbols.IParsable.Construct(argumentType)))
        {
            return TryParseMode.Parsable;
        }

        return TryParseMode.None;
    }

    private FormatMode GetFormatMode(string prefixName, ITypeSymbol argumentType, CancellationToken ct)
    {
        var tryFormatMethodName = $"TryFormat{prefixName}";
        var formatMethodName = $"Format{prefixName}";

        IMethodSymbol? tryFormatMethod = null;
        IMethodSymbol? formatMethod = null;
        foreach (var member in _typeSymbol.GetMembers(tryFormatMethodName))
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.Arity != 0)
            {
                continue;
            }

            if (methodSymbol.Parameters.Length != 5)
            {
                continue;
            }

            var param1 = methodSymbol.Parameters[0];
            var param2 = methodSymbol.Parameters[1];
            var param3 = methodSymbol.Parameters[2];
            var param4 = methodSymbol.Parameters[3];
            var param5 = methodSymbol.Parameters[4];

            if (!param1.Type.Equals(argumentType, SymbolEqualityComparer.Default))
            {
                continue;
            }

            if (!param2.Type.Equals(_symbols.SpanOfChar, SymbolEqualityComparer.Default))
            {
                continue;
            }

            if (!param3.Type.Equals(_symbols.Int, SymbolEqualityComparer.Default) || param3.RefKind != RefKind.Out)
            {
                continue;
            }

            if (!param4.Type.Equals(_symbols.ReadOnlySpanOfChar, SymbolEqualityComparer.Default))
            {
                continue;
            }

            if (!param5.Type.Equals(_symbols.IFormatProvider, SymbolEqualityComparer.Default))
            {
                continue;
            }

            // we have a match - check diagnostics later
            tryFormatMethod = methodSymbol;
            break;
        }

        foreach (var member in _typeSymbol.GetMembers(formatMethodName))
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.Arity != 0)
            {
                continue;
            }

            if (methodSymbol.Parameters.Length != 3)
            {
                continue;
            }

            var param1 = methodSymbol.Parameters[0];
            var param2 = methodSymbol.Parameters[1];
            var param3 = methodSymbol.Parameters[2];

            if (!param1.Type.Equals(argumentType, SymbolEqualityComparer.Default))
            {
                continue;
            }

            if (!param2.Type.Equals(_symbols.String, SymbolEqualityComparer.Default))
            {
                continue;
            }

            if (!param3.Type.Equals(_symbols.IFormatProvider, SymbolEqualityComparer.Default))
            {
                continue;
            }

            // we have a match - check diagnostics later
            formatMethod = methodSymbol;
            break;
        }

        switch (formatMethod, tryFormatMethod)
        {
            case (null, { } tryFormat):
                // we require a format method if we have a try-format method
                LocationInfo? location = null;
                foreach (var syntaxRef in tryFormat.DeclaringSyntaxReferences)
                {
                    location = LocationInfo.CreateFrom(syntaxRef, ct);
                    if (location is not null)
                    {
                        break;
                    }
                }

                AddDiagnostic(DiagnosticInfo.Create(DiagnosticDescriptors.UrnTypeMethodHasTryFormatButNoFormat, location));
                return FormatMode.None;

            case (not null, null):
                // we have a format method, but no try-format method
                return FormatMode.ExplicitlyDefinedFormatOnly;

            case (not null, not null):
                // we have both
                return FormatMode.ExplicitlyDefinedBoth;
        }

        if (argumentType.Implements(_symbols.IUrnFormattable))
        {
            return FormatMode.UrnFormattable;
        }

        if (argumentType.Implements(_symbols.ISpanFormattable))
        {
            return FormatMode.SpanFormattable;
        }

        if (argumentType.Implements(_symbols.IFormattable))
        {
            return FormatMode.Formattable;
        }

        return FormatMode.None;
    }
}
