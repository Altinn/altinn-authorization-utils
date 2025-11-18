using Altinn.Swashbuckle.Security;
using Altinn.Swashbuckle.Utils;
using System.Collections;
using System.Collections.Immutable;
using Xunit.Sdk;

namespace Altinn.Swashbuckle.Tests;

public class SecurityInfoTests
{
    [Theory]
    [MemberData(nameof(GetNormalizationCases))]
    public void Normalization_Check(
        SecurityInfo info,
        NormalizedSecurityInfo expectedNormalized)
    {
        var normalized = info.Normalized();
        normalized.Length.ShouldBe(expectedNormalized.Count);

        var remainingExpected = expectedNormalized
            .Select(inner => inner.ToImmutableSortedDictionary(kv => kv.Key, kv => kv.Value.ToImmutableSortedSet()))
            .ToList();

        var remainingNormalized = normalized
            .Select(inner => inner.ToImmutableSortedDictionary(kv => kv.Key, kv => kv.Value.ToImmutableSortedSet()))
            .ToList();

        remainingExpected.Sort(SortNormalized);
        remainingNormalized.Sort(SortNormalized);


        remainingExpected.Count.ShouldBe(remainingNormalized.Count);
        foreach (var (expectedInner, normalizedInner) in remainingExpected.Zip(remainingNormalized))
        {
            expectedInner.Count.ShouldBe(normalizedInner.Count);
            foreach (var (expectedKV, normalizedKV) in expectedInner.Zip(normalizedInner))
            {
                expectedKV.Key.ShouldBe(normalizedKV.Key);
                expectedKV.Value.Count.ShouldBe(normalizedKV.Value.Count);
                
                foreach (var (expectedScope, normalizedScope) in expectedKV.Value.Zip(normalizedKV.Value))
                {
                    expectedScope.ShouldBe(normalizedScope);
                }
            }
        }

        static int SortNormalized(
            ImmutableSortedDictionary<string, ImmutableSortedSet<string>> left,
            ImmutableSortedDictionary<string, ImmutableSortedSet<string>> right)
        {
            var order = Comparison.Equal
                .Then(left.Count, right.Count);

            if (order != Comparison.Equal)
            {
                return order;
            }

            foreach (var (lkvp, rkvp) in left.Zip(right))
            {
                order = order.Then(lkvp.Key, rkvp.Key);
            }

            if (order != Comparison.Equal)
            {
                return order;
            }

            foreach (var (lkvp, rkvp) in left.Zip(right))
            {
                order = order.Then(lkvp.Value.Count, rkvp.Value.Count);
            }

            if (order != Comparison.Equal)
            {
                return order;
            }

            foreach (var (lscope, rscope) in left.SelectMany(l => l.Value).Zip(right.SelectMany(r => r.Value)))
            {
                order = order.Then(lscope, rscope);
            }

            return order;
        }
    }

    public static TheoryData<SecurityInfo, NormalizedSecurityInfo> GetNormalizationCases()
    {
        var apimCondition = SecurityRequirementCondition.Create("apim");
        var apimRequirement = SecurityRequirement.Create("requires apim", [apimCondition]);

        var readScopeCondition = SecurityRequirementCondition.Create("scope", "read");
        var writeScopeCondition = SecurityRequirementCondition.Create("scope", "write");
        var adminScopeCondition = SecurityRequirementCondition.Create("scope", "admin");
        var searchScopeCondition = SecurityRequirementCondition.Create("scope", "search");
        var readRequirement = SecurityRequirement.Create("requires read access", [readScopeCondition, writeScopeCondition, adminScopeCondition]);
        var writeRequirement = SecurityRequirement.Create("requires write access", [writeScopeCondition, adminScopeCondition]);
        var searchRequirement = SecurityRequirement.Create("requires search access", [searchScopeCondition, adminScopeCondition]);
        var adminRequirement = SecurityRequirement.Create("requires admin access", [adminScopeCondition]);

        var cases = new TheoryData<SecurityInfo, NormalizedSecurityInfo>();
        
        Add(
            "empty", 
            [], 
            []);

        Add(
            "single-no-scope", 
            [apimRequirement], 
            [
                 // requires EITHER:
                [new("apim", [])], // - APIM
            ]);
        
        Add(
            "apim-read", 
            [apimRequirement, readRequirement], 
            [
                // requires EITHER:
                [new("apim", []), new("scope", ["read"])],  // - APIM and scope: read
                [new("apim", []), new("scope", ["write"])], // - APIM and scope: write
                [new("apim", []), new("scope", ["admin"])], // - APIM and scope: admin
            ]);

        Add(
            "apim-admin",
            [apimRequirement, adminRequirement],
            [
                // requires EITHER:
                [new("apim", []), new("scope", ["admin"])], // - APIM and scope: admin
            ]);

        Add(
            "apim-read-write",
            [apimRequirement, readRequirement, writeRequirement],
            [
                // requires EITHER:
                [new("apim", []), new("scope", ["write"])], // - APIM and scope: write
                [new("apim", []), new("scope", ["admin"])], // - APIM and scope: admin
            ]);

        Add(
            "apim-search",
            [apimRequirement, searchRequirement],
            [
                // requires EITHER:
                [new("apim", []), new("scope", ["search"])], // - APIM and scope: search
                [new("apim", []), new("scope", ["admin"])],  // - APIM and scope: admin
            ]);

        Add(
            "apim-search-write",
            [apimRequirement, searchRequirement, writeRequirement],
            [
                // requires EITHER:
                [new("apim", []), new("scope", ["search", "write"])], // - APIM and scope: search & write
                [new("apim", []), new("scope", ["admin"])],           // - APIM and scope: admin
            ]);

        return cases;

        void Add(
            string label,
            IEnumerable<SecurityRequirement> requirements,
            ImmutableArray<ImmutableArray<KeyValuePair<string, ImmutableArray<string>>>> expectedNormalized)
        {
            var info = SecurityInfo.Create(requirements);
            var normalized = new NormalizedSecurityInfo(expectedNormalized);
            var row = new TheoryDataRow<SecurityInfo, NormalizedSecurityInfo>(info, normalized)
            {
                Label = label,
            };

            cases.Add(row);
        }
    }

    public class NormalizedSecurityInfo
        : IXunitSerializable
        , IEnumerable<IEnumerable<KeyValuePair<string, IEnumerable<string>>>>
    {
        private ImmutableArray<ImmutableArray<KeyValuePair<string, ImmutableArray<string>>>> _data;

        public NormalizedSecurityInfo()
        {
        }

        public NormalizedSecurityInfo(ImmutableArray<ImmutableArray<KeyValuePair<string, ImmutableArray<string>>>> data)
        {
            _data = data;
        }

        public int Count => _data.Length;

        public IEnumerator<IEnumerable<KeyValuePair<string, IEnumerable<string>>>> GetEnumerator()
            => _data.Select(v => v.Select(kv => KeyValuePair.Create(kv.Key, kv.Value.AsEnumerable()))).GetEnumerator();

        void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
        {
            var count = info.GetValue<int>("c");
            var builder = ImmutableArray.CreateBuilder<ImmutableArray<KeyValuePair<string, ImmutableArray<string>>>>(count);

            for (var i = 0; i < count; i++)
            {
                builder.Add(DeserializeInner(info, $"{i}"));
            }

            _data = builder.DrainToImmutable();

            static ImmutableArray<KeyValuePair<string, ImmutableArray<string>>> DeserializeInner(IXunitSerializationInfo info, string prefix)
            {
                var count = info.GetValue<int>($"{prefix}:c");
                var builder = ImmutableArray.CreateBuilder<KeyValuePair<string, ImmutableArray<string>>>(count);

                for (var i = 0; i < count; i++)
                {
                    builder.Add(DeserializeKVP(info, $"{prefix}:{i}"));
                }

                return builder.DrainToImmutable();
            }

            static KeyValuePair<string, ImmutableArray<string>> DeserializeKVP(IXunitSerializationInfo info, string prefix)
            {
                var key = info.GetValue<string>($"{prefix}:s")!;

                var count = info.GetValue<int>($"{prefix}:c");
                var builder = ImmutableArray.CreateBuilder<string>(count);

                for (var i = 0; i < count; i++)
                {
                    builder.Add(info.GetValue<string>($"{prefix}:{i}")!);
                }

                return KeyValuePair.Create(key, builder.DrainToImmutable());
            }
        }

        void IXunitSerializable.Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("c", _data.Length, typeof(int));

            for (var i = 0; i < _data.Length; i++)
            {
                var inner = _data[i];
                info.AddValue($"{i}:c", inner.Length, typeof(int));

                for (var j = 0; j < inner.Length; j++)
                {
                    var kvp = inner[j];
                    info.AddValue($"{i}:{j}:s", kvp.Key, typeof(string));
                    info.AddValue($"{i}:{j}:c", kvp.Value.Length, typeof(int));

                    for (var k = 0; k < kvp.Value.Length; k++)
                    {
                        var scope = kvp.Value[k];
                        info.AddValue($"{i}:{j}:{k}", scope, typeof(string));
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
