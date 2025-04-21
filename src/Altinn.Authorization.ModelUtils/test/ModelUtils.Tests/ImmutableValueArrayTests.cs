using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Altinn.Authorization.ModelUtils.Tests;

public class ImmutableValueArrayTests
{
    #region Factories

    [Fact]
    public void CollectionExpressions()
    {
        AssertEqual<int>([], []);
        AssertEqual([1], [1]);
        AssertEqual([1, 2], [1, 2]);
        AssertEqual([.. Enumerable.Range(1, 10)], [.. Enumerable.Range(1, 10)]);
    }

    [Fact]
    public void ImmutableValueArray_Create_Empty()
    {
        AssertEqual([], ImmutableValueArray.Create<int>());
    }

    [Fact]
    public void ImmutableValueArray_Create_Single()
    {
        AssertEqual([1], ImmutableValueArray.Create(1));
    }

    [Fact]
    public void ImmutableValueArray_Create_Two()
    {
        AssertEqual([1, 2], ImmutableValueArray.Create(1, 2));
    }

    [Fact]
    public void ImmutableValueArray_Create_Three()
    {
        AssertEqual([1, 2, 3], ImmutableValueArray.Create(1, 2, 3));
    }

    [Fact]
    public void ImmutableValueArray_Create_Four()
    {
        AssertEqual([1, 2, 3, 4], ImmutableValueArray.Create(1, 2, 3, 4));
    }

    [Fact]
    public void ImmutableValueArray_Create_Span()
    {
        Span<int> values = [1, 2, 3, 4, 5];
        AssertEqual([.. values], ImmutableValueArray.Create(values));
    }

    [Fact]
    public void ImmutableValueArray_Create_ReadOnlySpan()
    {
        ReadOnlySpan<int> values = [1, 2, 3, 4, 5];
        AssertEqual([.. values], ImmutableValueArray.Create(values));
    }

    [Fact]
    public void ImmutableValueArray_Create_Array()
    {
        int[] values = [1, 2, 3, 4, 5];
        AssertEqual([.. values], ImmutableValueArray.Create(values));
    }

    [Fact]
    public void ImmutableValueArray_Create_Array_Slice()
    {
        int[] values = [1, 2, 3, 4, 5];
        AssertEqual([.. values.AsSpan().Slice(1, 2)], ImmutableValueArray.Create(values, 1, 2));
    }

    [Fact]
    public void ImmutableValueArray_Create_ImmutableArray()
    {
        ImmutableArray<int> values = [1, 2, 3, 4, 5];
        AssertEqual(values, ImmutableValueArray.Create(values));
    }

    [Fact]
    public void ImmutableValueArray_Create_ImmutableArray_Slice()
    {
        ImmutableArray<int> values = [1, 2, 3, 4, 5];
        AssertEqual([.. values.AsSpan().Slice(1, 2)], ImmutableValueArray.Create(values, 1, 2));
    }

    [Fact]
    public void ImmutableValueArray_Create_ImmutableValueArray()
    {
        ImmutableValueArray<int> values = [1, 2, 3, 4, 5];
        AssertEqual([.. values], ImmutableValueArray.Create(values));
    }

    [Fact]
    public void ImmutableValueArray_Create_ImmutableValueArray_Slice()
    {
        ImmutableValueArray<int> values = [1, 2, 3, 4, 5];
        AssertEqual([.. values.AsSpan().Slice(1, 2)], ImmutableValueArray.Create(values, 1, 2));
    }

    [Fact]
    public void ReadOnlySpan_ToImmutableValueArray()
    {
        ReadOnlySpan<int> values = [1, 2, 3, 4, 5];
        AssertEqual(values.ToImmutableArray(), values.ToImmutableValueArray());
    }

    [Fact]
    public void Span_ToImmutableValueArray()
    {
        Span<int> values = [1, 2, 3, 4, 5];
        AssertEqual(values.ToImmutableArray(), values.ToImmutableValueArray());
    }

    [Fact]
    public void ImmutableArray_ToImmutableValueArray()
    {
        ImmutableArray<int> values = [1, 2, 3, 4, 5];
        AssertEqual(values, values.ToImmutableValueArray());
    }

    [Fact]
    public void IEnumerable_ToImmutableValueArray()
    {
        IEnumerable<int> values = [1, 2, 3, 4, 5];
        AssertEqual(values.ToImmutableArray(), values.ToImmutableValueArray());
    }

    [Fact]
    public void Builder_ToImmutableValueArray()
    {
        AssertEqual(CreateBuilder().ToImmutable(), CreateBuilder().ToImmutableValueArray());
        static ImmutableArray<int>.Builder CreateBuilder() => ImmutableArray.Create(1, 2, 3, 4, 5).ToBuilder();
    }

    [Fact]
    public void Builder_MoveToImmutableValueArray_CorrectCapacity()
    {
        AssertEqual(CreateBuilder().MoveToImmutable(), CreateBuilder().MoveToImmutableValueArray());

        static ImmutableArray<int>.Builder CreateBuilder()
        {
            var builder = ImmutableArray.CreateBuilder<int>(5);
            builder.Add(1);
            builder.Add(2);
            builder.Add(3);
            builder.Add(4);
            builder.Add(5);
            return builder;
        }
    }

    [Fact]
    public void Builder_MoveToImmutableValueArray_WrongCapacity()
    {
        var builder = ImmutableArray.CreateBuilder<int>(5);
        Should.Throw<InvalidOperationException>(() => builder.MoveToImmutableValueArray());
    }

    [Fact]
    public void Builder_DrainToImmutableValueArray_CorrectCapacity()
    {
        AssertEqual(CreateBuilder().DrainToImmutable(), CreateBuilder().DrainToImmutableValueArray());

        var builder = CreateBuilder();
        builder.DrainToImmutableValueArray();
        builder.Count.ShouldBe(0);

        static ImmutableArray<int>.Builder CreateBuilder()
        {
            var builder = ImmutableArray.CreateBuilder<int>(5);
            builder.Add(1);
            builder.Add(2);
            builder.Add(3);
            builder.Add(4);
            builder.Add(5);
            return builder;
        }
    }

    [Fact]
    public void Builder_DrainToImmutableValueArray_WrongCapacity()
    {
        AssertEqual(CreateBuilder().DrainToImmutable(), CreateBuilder().DrainToImmutableValueArray());

        var builder = CreateBuilder();
        builder.DrainToImmutableValueArray();
        builder.Count.ShouldBe(0);

        static ImmutableArray<int>.Builder CreateBuilder()
        {
            var builder = ImmutableArray.CreateBuilder<int>(5);
            builder.Add(1);
            return builder;
        }
    }

    #endregion

    [Fact]
    public void ImmutableValueArray_Empty()
    {
        AssertEqual(ImmutableArray<int>.Empty, ImmutableValueArray<int>.Empty);
    }

    [Fact]
    public void ItemRef()
    {
        ImmutableArray<int> inner = [1, 2, 3, 4, 5];
        ImmutableValueArray<int> outer = ImmutableValueArray.Create(inner);

        for (int i = 0; i < inner.Length; i++)
        {
            ref readonly var innerRef = ref inner.ItemRef(i);
            ref readonly var outerRef = ref outer.ItemRef(i);

            Unsafe.AreSame(in innerRef, in outerRef).ShouldBeTrue();
        }
    }

    [Fact]
    public void IsEmpty()
    {
        ImmutableValueArray<int> def = default;
        ImmutableValueArray<int> empty = [];
        ImmutableValueArray<int> nonEmpty = [1, 2, 3];

        Should.Throw<NullReferenceException>(() => def.IsEmpty);
        empty.IsEmpty.ShouldBeTrue();
        nonEmpty.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void IsDefault()
    {
        ImmutableValueArray<int> def = default;
        ImmutableValueArray<int> empty = [];
        ImmutableValueArray<int> nonEmpty = [1, 2, 3];

        def.IsDefault.ShouldBeTrue();
        empty.IsDefault.ShouldBeFalse();
        nonEmpty.IsDefault.ShouldBeFalse();
    }

    [Fact]
    public void IsDefaultOrEmpty()
    {
        ImmutableValueArray<int> def = default;
        ImmutableValueArray<int> empty = [];
        ImmutableValueArray<int> nonEmpty = [1, 2, 3];

        def.IsDefaultOrEmpty.ShouldBeTrue();
        empty.IsDefaultOrEmpty.ShouldBeTrue();
        nonEmpty.IsDefaultOrEmpty.ShouldBeFalse();
    }

    [Fact]
    public void CopyTo_ExactSize()
    {
        ImmutableValueArray<int> source = [1, 2, 3, 4, 5];
        int[] destination = new int[5];
        source.CopyTo(destination);

        destination[0].ShouldBe(source[0]);
        destination[1].ShouldBe(source[1]);
        destination[2].ShouldBe(source[2]);
        destination[3].ShouldBe(source[3]);
        destination[4].ShouldBe(source[4]);
    }

    [Fact]
    public void CopyTo_Larger()
    {
        ImmutableValueArray<int> source = [1, 2, 3, 4, 5];
        int[] destination = new int[6];
        source.CopyTo(destination);

        destination[0].ShouldBe(source[0]);
        destination[1].ShouldBe(source[1]);
        destination[2].ShouldBe(source[2]);
        destination[3].ShouldBe(source[3]);
        destination[4].ShouldBe(source[4]);
        destination[5].ShouldBe(0);
    }

    [Fact]
    public void CopyTo_Smaller()
    {
        ImmutableValueArray<int> source = [1, 2, 3, 4, 5];
        int[] destination = new int[4];
        Should.Throw<ArgumentException>(() => source.CopyTo(destination));
    }

    [Fact]
    public void CopyTo_WithDestination()
    {
        ImmutableValueArray<int> source = [1, 2, 3, 4, 5];
        int[] destination = new int[6];
        source.CopyTo(destination, 1);

        destination[0].ShouldBe(0);
        destination[1].ShouldBe(source[0]);
        destination[2].ShouldBe(source[1]);
        destination[3].ShouldBe(source[2]);
        destination[4].ShouldBe(source[3]);
        destination[5].ShouldBe(source[4]);
    }

    [Fact]
    public void CopyTo_Slice()
    {
        ImmutableValueArray<int> source = [1, 2, 3, 4, 5];
        int[] destination = new int[3];
        source.CopyTo(2, destination, 1, 2);

        destination[0].ShouldBe(0);
        destination[1].ShouldBe(source[2]);
        destination[2].ShouldBe(source[3]);
    }

    [Fact]
    public void Enumerator()
    {
        ImmutableValueArray<int> source = [1, 2];
        ImmutableArray<int>.Enumerator enumerator = source.GetEnumerator();

        enumerator.MoveNext().ShouldBeTrue();
        enumerator.Current.ShouldBe(source[0]);

        enumerator.MoveNext().ShouldBeTrue();
        enumerator.Current.ShouldBe(source[1]);

        enumerator.MoveNext().ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void GetHashCode_WhenEquals(int[]? source)
    {
        ImmutableValueArray<int> left = From(source);
        ImmutableValueArray<int> right = From(source);

        left.GetHashCode().ShouldBe(right.GetHashCode());
        
        var comp = EqualityComparer<ImmutableValueArray<int>>.Default;
        comp.GetHashCode(left).ShouldBe(comp.GetHashCode(right));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Equals_WhenEquals(int[]? source)
    {
        ImmutableValueArray<int> left = From(source);
        ImmutableValueArray<int> right = From(source);

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
        EqualityComparer<ImmutableValueArray<int>>.Default.Equals(left, right).ShouldBeTrue();

        if (!left.IsDefault)
        {
            left.ShouldBe(right);
        }
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Equals_WhenNotEquals(int[]? source)
    {
        ImmutableValueArray<int> left = From(source);
        ImmutableValueArray<int> right = ImmutableValueArray.Create(source).Add(-1);

        left.Equals(right).ShouldBeFalse();
        (left == right).ShouldBeFalse();
        (left != right).ShouldBeTrue();
        EqualityComparer<ImmutableValueArray<int>>.Default.Equals(left, right).ShouldBeFalse();

        if (!left.IsDefault)
        {
            left.ShouldNotBe(right);
        }
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IList_GetIndex(int[]? source)
    {
        CheckInterfaceMethod(source, static (IList<int> list) => list[0]);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IReadOnlyList_GetIndex(int[]? source)
    {
        CheckInterfaceMethod(source, static (IReadOnlyList<int> list) => list[0]);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IList_SetIndex(int[]? source)
    {
        CheckInterfaceMethod(source, static (IList<int> list) => list[0] = 2);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ICollection_IsReadOnly(int[]? source)
    {
        CheckInterfaceMethod(source, static (ICollection<int> list) => list.IsReadOnly);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ICollection_Count(int[]? source)
    {
        CheckInterfaceMethod(source, static (ICollection<int> list) => list.Count);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IReadOnlyCollection_Count(int[]? source)
    {
        CheckInterfaceMethod(source, static (IReadOnlyCollection<int> list) => list.Count);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void AsSpan(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.AsSpan(),
            static v => v.AsSpan(),
            static (l, r) =>
            {
                l.Length.ShouldBe(r.Length);
                Unsafe.AreSame(in MemoryMarshal.GetReference(l), in MemoryMarshal.GetReference(r)).ShouldBeTrue();
            });
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void AsSpan_Slice(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.AsSpan(1, 2),
            static v => v.AsSpan(1, 2),
            static (l, r) =>
            {
                l.Length.ShouldBe(r.Length);
                Unsafe.AreSame(in MemoryMarshal.GetReference(l), in MemoryMarshal.GetReference(r)).ShouldBeTrue();
            });
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void AsMemory(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.AsMemory(),
            static v => v.AsMemory(),
            static (l, r) =>
            {
                l.Length.ShouldBe(r.Length);
                Unsafe.AreSame(in MemoryMarshal.GetReference(l.Span), in MemoryMarshal.GetReference(r.Span)).ShouldBeTrue();
            });
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IndexOf(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.IndexOf(2),
            static v => v.IndexOf(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IndexOf_WithStartIndex(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.IndexOf(2, 1),
            static v => v.IndexOf(2, 1));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IndexOf_WithStartIndexAndCount(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.IndexOf(2, 1, 3),
            static v => v.IndexOf(2, 1, 3));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IndexOf_WithStartIndexAndComparer(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.IndexOf(2, 1, equalityComparer: null),
            static v => v.IndexOf(2, 1, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IndexOf_WithStartIndexAndCountAndComparer(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.IndexOf(2, 1, 3, equalityComparer: null),
            static v => v.IndexOf(2, 1, 3, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void LastIndexOf(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.LastIndexOf(2),
            static v => v.LastIndexOf(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void LastIndexOf_WithStartIndex(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.LastIndexOf(2, 1),
            static v => v.LastIndexOf(2, 1));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void LastIndexOf_WithStartIndexAndCount(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.LastIndexOf(2, 1, 3),
            static v => v.LastIndexOf(2, 1, 3));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void LastIndexOf_WithStartIndexAndCountAndComparer(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.LastIndexOf(2, 1, 3, equalityComparer: null),
            static v => v.LastIndexOf(2, 1, 3, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Contains(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.Contains(2),
            static v => v.Contains(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Contains_WithComparer(int[]? source)
    {
        CheckSameResult(
            source,
            static v => v.Contains(2, equalityComparer: null),
            static v => v.Contains(2, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Insert(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Insert(1, 2),
            static v => v.Insert(1, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void InsertRange_IEnumerable(int[]? source)
    {
        IEnumerable<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.InsertRange(1, enumerable),
            v => v.InsertRange(1, enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void InsertRange_ImmutableArray(int[]? source)
    {
        ImmutableArray<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.InsertRange(1, enumerable),
            v => v.InsertRange(1, enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void InsertRange_ImmutableValueArray(int[]? source)
    {
        ImmutableValueArray<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.InsertRange(1, enumerable),
            v => v.InsertRange(1, enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void InsertRange_Array(int[]? source)
    {
        int[] enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.InsertRange(1, enumerable),
            v => v.InsertRange(1, enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void InsertRange_Span(int[]? source)
    {
        CheckMutation(
            source,
            static v => { ReadOnlySpan<int> enumerable = [1, 2]; return v.InsertRange(1, enumerable); },
            static v => { ReadOnlySpan<int> enumerable = [1, 2]; return v.InsertRange(1, enumerable); });
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Add(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Add(2),
            static v => v.Add(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void AddRange_IEnumerable(int[]? source)
    {
        IEnumerable<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.AddRange(enumerable),
            v => v.AddRange(enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void AddRange_Array(int[]? source)
    {
        int[] enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.AddRange(enumerable),
            v => v.AddRange(enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void AddRange_ImmutableArray(int[]? source)
    {
        ImmutableArray<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.AddRange(enumerable),
            v => v.AddRange(enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void AddRange_ImmutableValueArray(int[]? source)
    {
        ImmutableValueArray<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.AddRange(enumerable),
            v => v.AddRange(enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void AddRange_Span(int[]? source)
    {
        CheckMutation(
            source,
            v => { ReadOnlySpan<int> enumerable = [1, 2]; return v.AddRange(enumerable); },
            v => { ReadOnlySpan<int> enumerable = [1, 2]; return v.AddRange(enumerable); });
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void SetItem(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.SetItem(1, 2),
            static v => v.SetItem(1, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Replace(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Replace(3, 2),
            static v => v.Replace(3, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Replace_WithComparer(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Replace(3, 2, equalityComparer: null),
            static v => v.Replace(3, 2, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Remove(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Remove(2),
            static v => v.Remove(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Remove_WithComparer(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Remove(2, equalityComparer: null),
            static v => v.Remove(2, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveAt(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.RemoveAt(1),
            static v => v.RemoveAt(1));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.RemoveRange(1, 2),
            static v => v.RemoveRange(1, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange_IEnumerable(int[]? source)
    {
        IEnumerable<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.RemoveRange(enumerable),
            v => v.RemoveRange(enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange_IEnumerable_WithComparer(int[]? source)
    {
        IEnumerable<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.RemoveRange(enumerable, equalityComparer: null),
            v => v.RemoveRange(enumerable, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange_ImmutableArray(int[]? source)
    {
        ImmutableArray<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.RemoveRange(enumerable),
            v => v.RemoveRange(enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange_ImmutableArray_WithComparer(int[]? source)
    {
        ImmutableArray<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.RemoveRange(enumerable, equalityComparer: null),
            v => v.RemoveRange(enumerable, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange_ImmutableValueArray(int[]? source)
    {
        ImmutableValueArray<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.RemoveRange(enumerable),
            v => v.RemoveRange(enumerable));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange_ImmutableValueArray_WithComparer(int[]? source)
    {
        ImmutableValueArray<int> enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.RemoveRange(enumerable, equalityComparer: null),
            v => v.RemoveRange(enumerable, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange_Array_WithComparer(int[]? source)
    {
        int[] enumerable = [2, 3];
        CheckMutation(
            source,
            v => v.RemoveRange(enumerable, equalityComparer: null),
            v => v.RemoveRange(enumerable, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveRange_Span_WithComparer(int[]? source)
    {
        CheckMutation(
            source,
            v => { ReadOnlySpan<int> enumerable = [1, 2]; return v.RemoveRange(enumerable, equalityComparer: null); },
            v => { ReadOnlySpan<int> enumerable = [1, 2]; return v.RemoveRange(enumerable, equalityComparer: null); });
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void RemoveAll(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.RemoveAll(static x => x % 2 == 0),
            static v => v.RemoveAll(static x => x % 2 == 0));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Clear(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Clear(),
            static v => v.Clear());
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Sort(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Sort(),
            static v => v.Sort());
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Sort_WithComparison(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Sort(static (a, b) => 1 - Comparer<int>.Default.Compare(a, b)),
            static v => v.Sort(static (a, b) => 1 - Comparer<int>.Default.Compare(a, b)));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Sort_WithComparer(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Sort(comparer: null),
            static v => v.Sort(comparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Sort_WithSlice(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Sort(1, 3, comparer: null),
            static v => v.Sort(1, 3, comparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void Slice(int[]? source)
    {
        CheckMutation(
            source,
            static v => v.Slice(1, 3),
            static v => v.Slice(1, 3));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void SliceSyntax(int[]? source)
    {
        CheckMutation(
            source,
            static v => v[1..3],
            static v => v[1..3]);
    }

    #region Explicit interface methods

    [Theory]
    [MemberData(nameof(Variants))]
    public void IList_Insert(int[]? source)
    {
        CheckInterfaceMethod(source, (IList<int> list) => list.Insert(1, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IList_RemoveAt(int[]? source)
    {
        CheckInterfaceMethod(source, (IList<int> list) => list.RemoveAt(1));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ICollection_Add(int[]? source)
    {
        CheckInterfaceMethod(source, (ICollection<int> list) => list.Add(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ICollection_Clear(int[]? source)
    {
        CheckInterfaceMethod(source, (ICollection<int> list) => list.Clear());
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ICollection_Remove(int[]? source)
    {
        CheckInterfaceMethod(source, (ICollection<int> list) => list.Remove(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_Add(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list.Add(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_Clear(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list.Clear());
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_Insert(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list.Insert(1, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_Remove(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list.Remove(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_RemoveAt(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list.RemoveAt(1));
    }

    [Theory]

    [MemberData(nameof(Variants))]
    public void IListNonGeneric_IsFixedSize(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list.IsFixedSize);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_IsReadOnly(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list.IsReadOnly);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ICollectionNonGeneric_Count(int[]? source)
    {
        CheckInterfaceMethod(source, (ICollection list) => list.Count);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ICollectionNonGeneric_IsSynchronized(int[]? source)
    {
        CheckInterfaceMethod(source, (ICollection list) => list.IsSynchronized);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void ICollectionNonGeneric_SyncRoot(int[]? source)
    {
        CheckInterfaceMethod(source, (ICollection list) => list.SyncRoot);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_GetItem(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list[0]);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_SetItem(int[]? source)
    {
        CheckInterfaceMethod(source, (IList list) => list[0] = 2);
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_Clear(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.Clear());
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_Add(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.Add(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_AddRange(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.AddRange([2, 3]));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_Insert(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.Insert(1, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_InsertRange(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.InsertRange(1, [2, 3]));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_Remove(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.Remove(2, equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_RemoveAll(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.RemoveAll(static x => x % 2 == 0));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_RemoveRange_Enumerable(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.RemoveRange([1, 2], equalityComparer: null));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_RemoveRange_Slice(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.RemoveRange(1, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_RemoveAt(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.RemoveAt(1));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_SetItem(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.SetItem(1, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IImmutableList_Replace(int[]? source)
    {
        CheckInterfaceMutation(source, static list => list.Replace(3, 2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_Contains(int[]? source)
    {
        CheckInterfaceMethod(source, static (IList list) => list.Contains(2));
    }

    [Theory]
    [MemberData(nameof(Variants))]
    public void IListNonGeneric_IndexOf(int[]? source)
    {
        CheckInterfaceMethod(source, static (IList list) => list.IndexOf(2));
    }

    #endregion

    public static TheoryData<int[]?> Variants => new([
        null,
        [],
        [1],
        [2, 1, 3],
        [5, 2, 3, 4, 1],
    ]);

    private static ImmutableValueArray<int> From(int[]? values)
        => values switch
        {
            null => default,
            [] => [],
            _ => ImmutableValueArray.Create(values),
        };

    private static void CheckMutation(
        int[]? values,
        Func<ImmutableValueArray<int>, ImmutableValueArray<int>> valueImpl,
        Func<ImmutableArray<int>, ImmutableArray<int>> immutableImpl)
    {
        var outer = From(values);
        var inner = outer.ToImmutableArray();

        var outerOutcome = Attempt(outer, valueImpl);
        var innerOutcome = Attempt(inner, immutableImpl);

        if (innerOutcome.Exception is { } exn)
        {
            outerOutcome.Exception.ShouldNotBeNull();
            outerOutcome.Exception.ShouldBeOfType(exn.GetType());
            outerOutcome.Exception.Message.ShouldBe(exn.Message);
        }
        else
        {
            outerOutcome.Exception.ShouldBeNull();
            AssertEqual(innerOutcome.Value!, outerOutcome.Value!);
        }

        static Outcome<T> Attempt<T>(T value, Func<T, T> call)
        {
            try
            {
                return new Outcome<T>(call(value));
            }
            catch (Exception ex)
            {
                return new Outcome<T>(ex);
            }
        }
    }

    private static void CheckInterfaceMutation(
        int[]? values,
        Func<IImmutableList<int>, IImmutableList<int>> mutation)
    {
        var outer = From(values);
        var inner = outer.ToImmutableArray();

        var outerOutcome = Attempt(outer, mutation);
        var innerOutcome = Attempt(inner, mutation);

        if (innerOutcome.Exception is { } exn)
        {
            outerOutcome.Exception.ShouldNotBeNull();
            outerOutcome.Exception.ShouldBeOfType(exn.GetType());
            outerOutcome.Exception.Message.ShouldBe(exn.Message);
        }
        else
        {
            outerOutcome.Exception.ShouldBeNull();
            var mutatedInner = innerOutcome.Value.ShouldBeOfType<ImmutableArray<int>>();
            var mutatedOuter = outerOutcome.Value.ShouldBeOfType<ImmutableValueArray<int>>();
            AssertEqual(mutatedInner, mutatedOuter);
        }

        static Outcome<IImmutableList<int>> Attempt(IImmutableList<int> value, Func<IImmutableList<int>, IImmutableList<int>> call)
        {
            try
            {
                return new Outcome<IImmutableList<int>>(call(value));
            }
            catch (Exception ex)
            {
                return new Outcome<IImmutableList<int>>(ex);
            }
        }
    }

    private static void CheckSameResult<U>(
        int[]? values,
        Func<ImmutableValueArray<int>, U> valueImpl,
        Func<ImmutableArray<int>, U> immutableImpl)
        => CheckSameResult(values, valueImpl, immutableImpl, static (U a, U b) => a.ShouldBe(b));

    private static void CheckSameResult<U>(
        int[]? values,
        Func<ImmutableValueArray<int>, U> valueImpl,
        Func<ImmutableArray<int>, U> immutableImpl,
        Action<U, U> checkOutcome)
        where U : allows ref struct
    {
        var outer = From(values);
        var inner = outer.ToImmutableArray();

        var outerOutcome = Attempt(outer, valueImpl);
        var innerOutcome = Attempt(inner, immutableImpl);

        if (innerOutcome.Exception is { } exn)
        {
            outerOutcome.Exception.ShouldNotBeNull();
            outerOutcome.Exception.ShouldBeOfType(exn.GetType());
            outerOutcome.Exception.Message.ShouldBe(exn.Message);
        }
        else
        {
            outerOutcome.Exception.ShouldBeNull();
            checkOutcome(outerOutcome.Value!, innerOutcome.Value!);
        }

        static Outcome<U> Attempt<T>(T value, Func<T, U> call)
        {
            try
            {
                return new Outcome<U>(call(value));
            }
            catch (Exception ex)
            {
                return new Outcome<U>(ex);
            }
        }
    }

    private static void CheckInterfaceMethod<T>(int[]? values, Action<T> call)
        => CheckInterfaceMethod<T, object?>(values, iface => { call(iface); return null; });

    private static void CheckInterfaceMethod<T, U>(int[]? values, Func<T, U> call)
        => CheckInterfaceMethod(values, call, static (U a, U b) => a.ShouldBe(b));

    private static void CheckInterfaceMethod<T, U>(int[]? values, Func<T, U> call, Action<U, U> checkOutcome)
        where U : allows ref struct
    {
        var outer = From(values);
        var inner = outer.ToImmutableArray();

        var outerOutcome = Attempt((T)(object)outer, call);
        var innerOutcome = Attempt((T)(object)inner, call);

        if (innerOutcome.Exception is { } exn)
        {
            outerOutcome.Exception.ShouldNotBeNull();
            outerOutcome.Exception.ShouldBeOfType(exn.GetType());
            outerOutcome.Exception.Message.ShouldBe(exn.Message);
        }
        else
        {
            outerOutcome.Exception.ShouldBeNull();
            checkOutcome(outerOutcome.Value!, innerOutcome.Value!);
        }


        static Outcome<U> Attempt(T value, Func<T, U> call)
        {
            try
            {
                return new Outcome<U>(call(value));
            }
            catch (Exception ex)
            {
                return new Outcome<U>(ex);
            }
        }
    }

    private static void AssertEqual<T>(ImmutableArray<T> expected, ImmutableValueArray<T> actual)
    {
        actual.Length.ShouldBe(expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            actual[i].ShouldBe(expected[i]);
        }
    }

    private readonly ref struct Outcome<T>
        where T : allows ref struct
    {
        private readonly T? _value;
        private readonly Exception? _exception;

        public Outcome(T value)
        {
            _value = value;
            _exception = null;
        }

        public Outcome(Exception exception)
        {
            _value = default;
            _exception = exception;
        }

        public Exception? Exception => _exception;

        public T? Value => _value;
    }
}
