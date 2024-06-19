using Altinn.Urn.SourceGenerator.Parsing;

namespace Altinn.Urn.SourceGenerator.Emitting;

internal ref struct UrnRecordEmitter 
{
    public static string Emit(
        in UrnRecordInfo record, 
        string jsonConverterAttribute, 
        string jsonConverterConcreteType, 
        string jsonVariantConverterConcreteType, 
        CancellationToken cancellationToken)
    {
        using var builder = CodeStringBuilder.Rent();
        var emitter = new UrnRecordEmitter(builder, jsonConverterAttribute, jsonConverterConcreteType, jsonVariantConverterConcreteType);
        emitter.Emit(in record, cancellationToken);
        
        return builder.ToString();
    }

    private readonly CodeStringBuilder _builder;
    private readonly string _jsonConverterAttribute;
    private readonly string _jsonConverterConcreteType;
    private readonly string _jsonVariantConverterConcreteType;

    private UrnRecordEmitter(
        CodeStringBuilder builder, 
        string jsonConverterAttribute, 
        string jsonConverterConcreteType,
        string jsonVariantConverterConcreteType)
    {
        _builder = builder;
        _jsonConverterAttribute = jsonConverterAttribute;
        _jsonConverterConcreteType = jsonConverterConcreteType;
        _jsonVariantConverterConcreteType = jsonVariantConverterConcreteType;
    }

    private void Emit(in UrnRecordInfo record, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var builder = new CodeBuilder(_builder);
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("using Altinn.Urn;");
        builder.AppendLine("using Altinn.Urn.Visit;");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Immutable;");
        builder.AppendLine("using System.ComponentModel;");
        builder.AppendLine("using System.Diagnostics;");
        builder.AppendLine("using System.Diagnostics.CodeAnalysis;");
        builder.AppendLine("using System.Runtime.CompilerServices;");
        builder.AppendLine();
        builder.AppendLine($"namespace {record.Namespace};");
        builder.AppendLine();

        var containingTypes = record.ContainingTypes.AsSpan();
        EmitContainingTypes(containingTypes, in record, builder, ct);
    }

    private void EmitContainingTypes(ReadOnlySpan<ContainingType> containingType, in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        if (containingType.IsEmpty)
        {
            EmitUrnRecord(record, builder, ct);
            return;
        }

        var outer = containingType[0];
        var inner = containingType[1..];

        builder.AppendLine($"partial {outer.Keyword} {outer.Name}");
        builder.AppendLine("{");
        
        EmitContainingTypes(inner, in record, builder.Indent(), ct);
        
        builder.AppendLine("}");
    }

    private void EmitUrnRecord(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        builder.AppendLine("""[DebuggerDisplay("{DebuggerDisplay}")]""");
        builder.AppendLine($"[{_jsonConverterAttribute}(typeof({_jsonConverterConcreteType}))]");
        builder.AppendLine($"partial {record.Keyword} {record.TypeName}");
        builder.AppendLine($"    : IParsable<{record.TypeName}>");
        builder.AppendLine($"    , ISpanParsable<{record.TypeName}>");
        builder.AppendLine($"    , IFormattable");
        builder.AppendLine($"    , ISpanFormattable");
        builder.AppendLine($"    , IKeyValueUrn<{record.TypeName}, {record.TypeName}.Type>");
        builder.AppendLine($"    , IVisitableKeyValueUrn");
        builder.AppendLine("{");

        var indented = builder.Indent();
        EmitRecordMembers(in record, indented, ct);
        
        builder.AppendLine();
        EmitRecordOverrides(in record, indented, ct);
        EmitTypeParsers(in record, indented, ct);
        EmitTypeFormatters(in record, indented, ct);
        EmitParseImpls(in record, indented, ct);
        EmitUtils(in record, indented, ct);

        builder.AppendLine();
        EmitTypeEnum(in record, indented, ct);
        EmitTypeRecords(in record, indented, ct);
        
        builder.AppendLine("}");
    }

    private void EmitRecordMembers(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var members = record.Members.AsSpan();
        var builder_lv1 = builder.Indent();
        var builder_lv2 = builder_lv1.Indent();
        var builder_lv3 = builder_lv2.Indent();
        var builder_lv4 = builder_lv3.Indent();

        builder.AppendLine("private static readonly ImmutableArray<Type> _variants = [");
        foreach (var member in members)
        {
            builder_lv1.AppendLine($"Type.{member.Name},");
        }
        builder.AppendLine("];");

        builder.AppendLine();
        builder.AppendLine("private static readonly ImmutableArray<string> _validPrefixes = [");
        foreach (var member in members)
        {
            foreach (var prefix in member.Prefixes)
            {
                builder_lv1.AppendLine($"\"urn:{prefix}\",");
            }
        }
        builder.AppendLine("];");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("public static ReadOnlySpan<Type> Variants => _variants.AsSpan();");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("public static ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static ReadOnlySpan<string> PrefixesFor(Type type)");
        builder_lv1.AppendLine("=> type switch {");
        foreach (var member in members)
        {
            builder_lv2.AppendLine($"Type.{member.Name} => {member.Name}.Prefixes,");
        }
        builder_lv2.AppendLine("_ => throw new ArgumentOutOfRangeException(nameof(type)),");
        builder_lv1.AppendLine("};");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static string CanonicalPrefixFor(Type type)");
        builder_lv1.AppendLine("=> type switch {");
        foreach (var member in members)
        {
            builder_lv2.AppendLine($"Type.{member.Name} => {member.Name}.CanonicalPrefix,");
        }
        builder_lv2.AppendLine("_ => throw new ArgumentOutOfRangeException(nameof(type)),");
        builder_lv1.AppendLine("};");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static System.Type ValueTypeFor(Type type)");
        builder_lv1.AppendLine("=> type switch {");
        foreach (var member in members)
        {
            builder_lv2.AppendLine($"Type.{member.Name} => typeof({member.ValueType}),");
        }
        builder_lv2.AppendLine("_ => throw new ArgumentOutOfRangeException(nameof(type)),");
        builder_lv1.AppendLine("    };");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static System.Type VariantTypeFor(Type type)");
        builder_lv1.AppendLine("=> type switch {");
        foreach (var member in members)
        {
            builder_lv2.AppendLine($"Type.{member.Name} => typeof({member.Name}),");
        }
        builder_lv2.AppendLine("_ => throw new ArgumentOutOfRangeException(nameof(type)),");
        builder_lv1.AppendLine("};");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static string NameFor(Type type)");
        builder_lv1.AppendLine("=> type switch {");
        foreach (var member in members)
        {
            builder_lv2.AppendLine($"Type.{member.Name} => nameof({member.Name}),");
        }
        builder_lv2.AppendLine("_ => throw new ArgumentOutOfRangeException(nameof(type)),");
        builder_lv1.AppendLine("};");

        builder.AppendLine();
        builder.AppendLine("private readonly KeyValueUrn _urn;");
        builder.AppendLine("private readonly Type _type;");

        builder.AppendLine();
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"private {record.TypeName}(string urn, int valueIndex, Type type) => (_urn, _type) = (KeyValueUrn.CreateUnchecked(urn, valueIndex), type);");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public string Urn => _urn.Urn;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public Type UrnType => _type;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public ReadOnlySpan<char> AsSpan() => _urn.AsSpan();");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public ReadOnlyMemory<char> AsMemory() => _urn.AsMemory();");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public ReadOnlySpan<char> ValueSpan => _urn.ValueSpan;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public ReadOnlyMemory<char> ValueMemory => _urn.ValueMemory;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public ReadOnlySpan<char> KeySpan => _urn.KeySpan;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public ReadOnlyMemory<char> KeyMemory => _urn.KeyMemory;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public ReadOnlySpan<char> PrefixSpan => _urn.PrefixSpan;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public ReadOnlyMemory<char> PrefixMemory => _urn.PrefixMemory;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public string ToString(string? format, IFormatProvider? provider)");
        builder.AppendLine("{");
        builder_lv1.AppendLine("switch (format.AsSpan())");
        builder_lv1.AppendLine("{");
        builder_lv2.AppendLine("case ['V', ..var valueFormatSpan]:");
        builder_lv3.AppendLine("var valueFormat = valueFormatSpan.Length == 0 ? null : new string(valueFormatSpan);");
        builder_lv3.AppendLine("return _type switch");
        builder_lv3.AppendLine("{");
        foreach (var member in members)
        {
            builder_lv4.AppendLine($"Type.{member.Name} => Format{member.Name}((({member.Name})this).Value, valueFormat, provider),");
        }
        builder_lv4.AppendLine("_ => Unreachable<string>(),");
        builder_lv3.AppendLine("};");
        builder_lv2.AppendLine();
        builder_lv2.AppendLine("default:");
        builder_lv3.AppendLine("return _urn.ToString(format, provider);");
        builder_lv1.AppendLine("}");
        builder.AppendLine("}");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)");
        builder.AppendLine("{");
        builder_lv1.AppendLine("switch (format)");
        builder_lv1.AppendLine("{");
        builder_lv2.AppendLine("case ['V', ..var valueFormatSpan]:");
        builder_lv3.AppendLine("charsWritten = 0;");
        builder_lv3.AppendLine("return _type switch");
        builder_lv3.AppendLine("{");
        foreach (var member in members)
        {
            if (member.FormatMode.TryFormatSupport)
            {
                builder_lv4.AppendLine($"Type.{member.Name} => TryFormat{member.Name}((({member.Name})this).Value, destination, out charsWritten, valueFormatSpan, provider),");
            }
            else
            {
                builder_lv4.AppendLine($"Type.{member.Name} => false,");
            }
        }
        builder_lv4.AppendLine("_ => Unreachable<bool>(),");
        builder_lv3.AppendLine("};");
        builder_lv2.AppendLine();
        builder_lv2.AppendLine("default:");
        builder_lv3.AppendLine("return _urn.TryFormat(destination, out charsWritten, format, provider);");
        builder_lv1.AppendLine("}");
        builder.AppendLine("}");

        builder.AppendLine();
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("protected abstract void Accept(IKeyValueUrnVisitor visitor);");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("void IVisitableKeyValueUrn.Accept(IKeyValueUrnVisitor visitor) => Accept(visitor);");

        foreach (var member in members)
        {
            builder.AppendLine();
            // CS8826 - our method here does not match the signature because we add the MaybeNullWhen attribute - this is on purpose
            builder.AppendLine("#pragma warning disable CS8826");
            builder.AppendLine("[CompilerGenerated]");
            builder.AppendLine($"public partial bool Is{member.Name}([MaybeNullWhen(false)] out {member.ValueType} value)");
            builder.AppendLine("#pragma warning restore CS8826");
            builder.AppendLine("{");
            builder_lv1.AppendLine($"if (_type == Type.{member.Name})");
            builder_lv1.AppendLine("{");
            builder_lv2.AppendLine($"value = (({member.Name})this).Value;");
            builder_lv2.AppendLine("return true;");
            builder_lv1.AppendLine("}");
            builder.AppendLine();
            builder_lv1.AppendLine("value = default;");
            builder_lv1.AppendLine("return false;");
            builder.AppendLine("}");
        }
    }

    private void EmitRecordOverrides(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("public override string ToString() => _urn.Urn;");

        builder.AppendLine();
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("protected string DebuggerDisplay => _urn.Urn;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public virtual bool Equals({record.TypeName}? other) => other is not null && _type == other._type && _urn == other._urn;");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("public override int GetHashCode() => _urn.GetHashCode();");
    }

    private void EmitTypeParsers(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        var builder_lv1 = builder.Indent();

        foreach (var type in record.Members)
        {
            if (!type.TryParseMode.Generate)
            {
                continue;
            }

            builder.AppendLine();
            builder.AppendLine("[CompilerGenerated]");
            builder.AppendLine($"private static bool TryParse{type.Name}(ReadOnlySpan<char> segment, IFormatProvider? provider, [MaybeNullWhen(false)] out {type.ValueType} value)"); 
            builder_lv1.AppendLine($"=> {type.ValueType}.TryParse(segment, provider, out value);");
        }
    }

    private void EmitTypeFormatters(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var builder_lv1 = builder.Indent();

        foreach (var type in record.Members)
        {
            if (!type.FormatMode.Generate)
            {
                continue;
            }

            builder.AppendLine();
            builder.AppendLine("[CompilerGenerated]");
            builder.AppendLine($"private static string Format{type.Name}({type.ValueType} value, string? format, IFormatProvider? provider)");
            builder_lv1.AppendLine($"=> (value as IFormattable).ToString(format, provider);");

            if (!type.FormatMode.TryFormatSupport)
            {
                continue;
            }

            builder.AppendLine();
            builder.AppendLine("[CompilerGenerated]");
            builder.AppendLine($"private static bool TryFormat{type.Name}({type.ValueType} value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)");
            builder_lv1.AppendLine($"=> (value as ISpanFormattable).TryFormat(destination, out charsWritten, format, provider);");
        }
    }

    private void EmitParseImpls(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        var typeName = record.TypeName;
        var prefixTree = new PrefixTree<UrnPrefixInfo>();
        foreach (var type in record.Members)
        {
            foreach (var prefix in type.Prefixes)
            {
                prefixTree.Add("urn:" + prefix, type);
            }
        }

        var flattened = prefixTree.Flatten();

        var builder_lv1 = builder.Indent();
        var builder_lv2 = builder_lv1.Indent();

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("public static bool TryGetVariant(ReadOnlySpan<char> prefix, [MaybeNullWhen(returnValue: false)] out Type variant)");
        builder.AppendLine("{");
        builder_lv1.AppendLine("ReadOnlySpan<char> s = prefix;");
        EmitPrefixChecks(builder_lv1, flattened, "s", "variant", parse: false);
        ct.ThrowIfCancellationRequested();
        builder.AppendLine("}");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("public static bool TryGetVariant(string prefix, [MaybeNullWhen(returnValue: false)] out Type variant)");
        builder_lv1.AppendLine("=> TryGetVariant(prefix.AsSpan(), out variant);");

        builder.AppendLine();
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, string? original, [MaybeNullWhen(false)] out {typeName} result)");
        builder.AppendLine("{");
        EmitPrefixChecks(builder_lv1, flattened, "s", "result", parse: true);
        ct.ThrowIfCancellationRequested();
        builder.AppendLine("}");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out {typeName} result)");
        builder_lv1.AppendLine("=> TryParse(s, provider, original: null, out result);");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out {typeName} result)");
        builder_lv1.AppendLine("=> TryParse(s, provider: null, original: null, out result);");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"""public static {typeName} Parse(ReadOnlySpan<char> s, IFormatProvider? provider)""");
        builder.AppendLine("{");
        builder_lv1.AppendLine($"if (!TryParse(s, provider, original: null, out {typeName}? result))");
        builder_lv1.AppendLine("{");
        builder_lv2.AppendLine($"""throw new FormatException("Could not parse {typeName}");""");
        builder_lv1.AppendLine("}");
        builder_lv1.AppendLine();
        builder_lv1.AppendLine("return result;");
        builder.AppendLine("}");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"""public static {typeName} Parse(ReadOnlySpan<char> s)""");
        builder.AppendLine("{");
        builder_lv1.AppendLine($"if (!TryParse(s, provider: null, original: null, out {typeName}? result))");
        builder_lv1.AppendLine("{");
        builder_lv2.AppendLine($"""throw new FormatException("Could not parse {typeName}");""");
        builder_lv1.AppendLine("}");
        builder_lv1.AppendLine();
        builder_lv1.AppendLine("return result;");
        builder.AppendLine("}");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out {typeName} result)");
        builder_lv1.AppendLine($"""=> TryParse(s.AsSpan(), provider, original: s, out result);""");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out {typeName} result)");
        builder_lv1.AppendLine($"""=> TryParse(s.AsSpan(), provider: null, original: s, out result);""");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static {typeName} Parse(string? s, IFormatProvider? provider)");
        builder.AppendLine("{");
        builder_lv1.AppendLine("ArgumentNullException.ThrowIfNull(s);");
        builder_lv1.AppendLine();
        builder_lv1.AppendLine($"if (!TryParse(s.AsSpan(), provider, original: s, out {typeName}? result))");
        builder_lv1.AppendLine("{");
        builder_lv2.AppendLine($"""throw new FormatException("Could not parse {typeName}");""");
        builder_lv1.AppendLine("}");
        builder_lv1.AppendLine();
        builder_lv1.AppendLine("return result;");
        builder.AppendLine("}");

        builder.AppendLine();
        builder.AppendLine("/// <inheritdoc/>");
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine($"public static {typeName} Parse(string? s)");
        builder.AppendLine("{");
        builder_lv1.AppendLine("ArgumentNullException.ThrowIfNull(s);");
        builder_lv1.AppendLine();
        builder_lv1.AppendLine($"if (!TryParse(s.AsSpan(), provider: null, original: s, out {typeName}? result))");
        builder_lv1.AppendLine("{");
        builder_lv2.AppendLine($"""throw new FormatException("Could not parse {typeName}");""");
        builder_lv1.AppendLine("}");
        builder_lv1.AppendLine();
        builder_lv1.AppendLine("return result;");
        builder.AppendLine("}");

        static void EmitPrefixChecks(CodeBuilder builder, IPrefixNode<UrnPrefixInfo> node, string spanName, string outVar, bool parse)
        {
            var builder_lv1 = builder.Indent();

            var index = 0;
            var firstLine = true;
            foreach (var child in node)
            {
                if (firstLine)
                {
                    firstLine = false;
                }
                else
                {
                    builder.AppendLine();
                }

                var sliceName = $"{spanName}_{index++}";
                builder.AppendLine($"""if ({spanName}.StartsWith("{child.Prefix}"))""");
                builder.AppendLine("{");
                builder_lv1.AppendLine($"var {sliceName} = {spanName}.Slice({child.Prefix.Length});");

                EmitPrefixChecks(builder_lv1, child, sliceName, outVar, parse);
                builder.AppendLine("}");
            }

            if (node.Value.HasValue)
            {
                if (firstLine)
                {
                    firstLine = false;
                }
                else
                {
                    builder.AppendLine();
                }

                var value = node.Value.Value;
                var outType = value.ValueTypeIsValueType ? value.ValueType : $"{value.ValueType}?";

                builder.AppendLine();
                if (parse)
                {
                    builder.AppendLine($"if ({spanName}.Length > 1 && {spanName}[0] == ':' && TryParse{value.Name}({spanName}.Slice(1), provider, out {outType} {spanName}_value))");
                    builder.AppendLine("{");
                    builder_lv1.AppendLine($"{outVar} = {value.Name}.FromParsed(original ?? new string(s), {node.PathLength + 1}, {spanName}_value);");
                    builder_lv1.AppendLine("return true;");
                    builder.AppendLine("}");
                }
                else
                {
                    builder.AppendLine($"if ({spanName}.Length == 0)");
                    builder.AppendLine("{");
                    builder_lv1.AppendLine($"{outVar} = Type.{value.Name};");
                    builder_lv1.AppendLine("return true;");
                    builder.AppendLine("}");
                }
            }

            if (!firstLine)
            {
                builder.AppendLine();
            }

            builder.AppendLine($"{outVar} = default;");
            builder.AppendLine("return false;");
        }
    }

    private void EmitUtils(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        builder.AppendLine();
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("private static T Unreachable<T>() => throw new UnreachableException();");
    }

    private void EmitTypeEnum(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("public enum Type");
        builder.AppendLine("{");

        var value = 1;
        var inner = builder.Indent();
        foreach (var type in record.Members)
        {
            inner.AppendLine($"{type.Name} = {value++},");
        }

        builder.AppendLine("}");
    }

    private void EmitTypeRecords(in UrnRecordInfo record, CodeBuilder builder, CancellationToken ct)
    {
        foreach (var member in record.Members)
        {
            ct.ThrowIfCancellationRequested();
            EmitTypeRecord(in record, in member, builder);
        }
    }

    private void EmitTypeRecord(in UrnRecordInfo record, in UrnPrefixInfo member, CodeBuilder builder)
    {
        var builder_lv1 = builder.Indent();
        var builder_lv2 = builder_lv1.Indent();
        var builder_lv3 = builder_lv2.Indent();
        var canonicalPrefix = $"urn:{member.Prefixes[member.CanonicalPrefixIndex]}";

        builder.AppendLine();
        builder.AppendLine("[CompilerGenerated]");
        builder.AppendLine("""[DebuggerDisplay("{DebuggerDisplay}")]""");
        builder.AppendLine($"[{_jsonConverterAttribute}(typeof({_jsonVariantConverterConcreteType}))]");
        builder.AppendLine($"public sealed partial record {member.Name}");
        builder_lv1.AppendLine($": {record.TypeName}");
        builder_lv1.AppendLine($", IKeyValueUrnVariant<{member.Name}, {record.TypeName}, Type, {member.ValueType}>");
        builder.AppendLine("{");

        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine($"public const string CanonicalPrefix = \"{canonicalPrefix}\";");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("/// <inheritdoc/>");
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine($"public static Type Variant => Type.{member.Name};");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("private static readonly new ImmutableArray<string> _validPrefixes = [");

        foreach (var prefix in member.Prefixes)
        {
            builder_lv2.AppendLine($"\"urn:{prefix}\",");
        }

        builder_lv1.AppendLine("];");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine("public static new ReadOnlySpan<string> Prefixes => _validPrefixes.AsSpan();");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine($"private readonly {member.ValueType} _value;");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine($"private {member.Name}(string urn, int valueIndex, {member.ValueType} value) : base(urn, valueIndex, Type.{member.Name}) => (_value) = (value);");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine("[EditorBrowsable(EditorBrowsableState.Never)]");
        builder_lv1.AppendLine($"internal static {member.Name} FromParsed(string urn, int valueIndex, {member.ValueType} value) => new(urn, valueIndex, value);");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine($"public {member.ValueType} Value => _value;");

        builder_lv1.AppendLine("/// <inheritdoc/>");
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine("public override string ToString() => _urn.Urn;");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("/// <inheritdoc/>");
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine($"public bool Equals({member.Name}? other) => other is not null && _urn == other._urn;");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("/// <inheritdoc/>");
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine("public override int GetHashCode() => _urn.GetHashCode();");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("/// <inheritdoc/>");
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine($"public static {member.Name} Create({member.ValueType} value)");
        builder_lv2.AppendLine($$""""=> new($"""{{canonicalPrefix}}:{new _FormatHelper(value)}""", {{canonicalPrefix.Length + 1}}, value);"""");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine("protected override void Accept(IKeyValueUrnVisitor visitor)");
        builder_lv2.AppendLine($"=> visitor.Visit<{record.TypeName}, Type, {member.ValueType}>(this, _type, _value);");

        builder_lv1.AppendLine();
        builder_lv1.AppendLine("[CompilerGenerated]");
        builder_lv1.AppendLine("[EditorBrowsable(EditorBrowsableState.Never)]");
        builder_lv1.AppendLine("private readonly struct _FormatHelper");
        builder_lv2.AppendLine(": IFormattable");
        if (member.FormatMode.TryFormatSupport)
        {
            builder_lv2.AppendLine(", ISpanFormattable");
        }
        builder_lv1.AppendLine("{");
        builder_lv2.AppendLine($"private readonly {member.ValueType} _value;");

        builder_lv2.AppendLine();
        builder_lv2.AppendLine($"public _FormatHelper({member.ValueType} value) => _value = value;");

        if (member.FormatMode.TryFormatSupport)
        {
            builder_lv2.AppendLine();
            builder_lv2.AppendLine("public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)");
            builder_lv3.AppendLine($"=> TryFormat{member.Name}(_value, destination, out charsWritten, format, provider);");
        }

        builder_lv2.AppendLine();
        builder_lv2.AppendLine("public string ToString(string? format, IFormatProvider? provider)");
        builder_lv3.AppendLine($"=> Format{member.Name}(_value, format, provider);");
        builder_lv1.AppendLine("}");

        builder.AppendLine("}");
    }
}
