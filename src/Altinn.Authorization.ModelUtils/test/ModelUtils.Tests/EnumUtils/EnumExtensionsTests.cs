using Altinn.Authorization.ModelUtils.EnumUtils;

namespace Altinn.Authorization.ModelUtils.Tests.EnumUtils;

public class EnumExtensionsTests
{
    [Fact]
    public void IsDefault()
    {
        Bytes.None.IsDefault().ShouldBeTrue();
        SBytes.None.IsDefault().ShouldBeTrue();
        Shorts.None.IsDefault().ShouldBeTrue();
        UShorts.None.IsDefault().ShouldBeTrue();
        Ints.None.IsDefault().ShouldBeTrue();
        UInts.None.IsDefault().ShouldBeTrue();
        Longs.None.IsDefault().ShouldBeTrue();
        ULongs.None.IsDefault().ShouldBeTrue();

        Bytes.A.IsDefault().ShouldBeFalse();
        SBytes.A.IsDefault().ShouldBeFalse();
        Shorts.A.IsDefault().ShouldBeFalse();
        UShorts.A.IsDefault().ShouldBeFalse();
        Ints.A.IsDefault().ShouldBeFalse();
        UInts.A.IsDefault().ShouldBeFalse();
        Longs.A.IsDefault().ShouldBeFalse();
        ULongs.A.IsDefault().ShouldBeFalse();
    }

    [Fact]
    public void HasAnyFlags()
    {
        CheckImpl(Bytes.None, Bytes.A, Bytes.B, Bytes.C, Bytes.AB, Bytes.BC, Bytes.AC, Bytes.ABC);
        CheckImpl(SBytes.None, SBytes.A, SBytes.B, SBytes.C, SBytes.AB, SBytes.BC, SBytes.AC, SBytes.ABC);
        CheckImpl(Shorts.None, Shorts.A, Shorts.B, Shorts.C, Shorts.AB, Shorts.BC, Shorts.AC, Shorts.ABC);
        CheckImpl(UShorts.None, UShorts.A, UShorts.B, UShorts.C, UShorts.AB, UShorts.BC, UShorts.AC, UShorts.ABC);
        CheckImpl(Ints.None, Ints.A, Ints.B, Ints.C, Ints.AB, Ints.BC, Ints.AC, Ints.ABC);
        CheckImpl(UInts.None, UInts.A, UInts.B, UInts.C, UInts.AB, UInts.BC, UInts.AC, UInts.ABC);
        CheckImpl(Longs.None, Longs.A, Longs.B, Longs.C, Longs.AB, Longs.BC, Longs.AC, Longs.ABC);
        CheckImpl(ULongs.None, ULongs.A, ULongs.B, ULongs.C, ULongs.AB, ULongs.BC, ULongs.AC, ULongs.ABC);

        static void CheckImpl<T>(
            T none,
            T a,
            T b,
            T c,
            T ab,
            T bc,
            T ac,
            T abc)
            where T : struct, Enum
        {
            a.HasAnyFlags(none).ShouldBeFalse();
            b.HasAnyFlags(none).ShouldBeFalse();
            c.HasAnyFlags(none).ShouldBeFalse();

            none.HasAnyFlags(a).ShouldBeFalse();
            none.HasAnyFlags(b).ShouldBeFalse();
            none.HasAnyFlags(c).ShouldBeFalse();

            ab.HasAnyFlags(a).ShouldBeTrue();
            ab.HasAnyFlags(b).ShouldBeTrue();
            ab.HasAnyFlags(c).ShouldBeFalse();
            ab.HasAnyFlags(ab).ShouldBeTrue();
            ab.HasAnyFlags(bc).ShouldBeTrue();
            ab.HasAnyFlags(ac).ShouldBeTrue();
            ab.HasAnyFlags(abc).ShouldBeTrue();

            a.HasAnyFlags(abc).ShouldBeTrue();
            a.HasAnyFlags(bc).ShouldBeFalse();
        }
    }

    [Fact]
    public void BitwiseOr()
    {
        CheckImpl(Bytes.None, Bytes.A, Bytes.B, Bytes.C, Bytes.AB, Bytes.BC, Bytes.AC, Bytes.ABC);
        CheckImpl(SBytes.None, SBytes.A, SBytes.B, SBytes.C, SBytes.AB, SBytes.BC, SBytes.AC, SBytes.ABC);
        CheckImpl(Shorts.None, Shorts.A, Shorts.B, Shorts.C, Shorts.AB, Shorts.BC, Shorts.AC, Shorts.ABC);
        CheckImpl(UShorts.None, UShorts.A, UShorts.B, UShorts.C, UShorts.AB, UShorts.BC, UShorts.AC, UShorts.ABC);
        CheckImpl(Ints.None, Ints.A, Ints.B, Ints.C, Ints.AB, Ints.BC, Ints.AC, Ints.ABC);
        CheckImpl(UInts.None, UInts.A, UInts.B, UInts.C, UInts.AB, UInts.BC, UInts.AC, UInts.ABC);
        CheckImpl(Longs.None, Longs.A, Longs.B, Longs.C, Longs.AB, Longs.BC, Longs.AC, Longs.ABC);
        CheckImpl(ULongs.None, ULongs.A, ULongs.B, ULongs.C, ULongs.AB, ULongs.BC, ULongs.AC, ULongs.ABC);

        static void CheckImpl<T>(
            T none,
            T a,
            T b,
            T c,
            T ab,
            T bc,
            T ac,
            T abc)
            where T : struct, Enum
        {
            a.BitwiseOr(b).ShouldBe(ab);
            a.BitwiseOr(c).ShouldBe(ac);
            b.BitwiseOr(c).ShouldBe(bc);

            a.BitwiseOr(ab).ShouldBe(ab);
            a.BitwiseOr(ac).ShouldBe(ac);
            a.BitwiseOr(abc).ShouldBe(abc);

            a.BitwiseOr(none).ShouldBe(a);
            a.BitwiseOr(a).ShouldBe(a);

            none.BitwiseOr(none).ShouldBe(none);
        }
    }

    [Fact]
    public void RemoveFlags()
    {
        CheckImpl(Bytes.None, Bytes.A, Bytes.B, Bytes.C, Bytes.AB, Bytes.BC, Bytes.AC, Bytes.ABC);
        CheckImpl(SBytes.None, SBytes.A, SBytes.B, SBytes.C, SBytes.AB, SBytes.BC, SBytes.AC, SBytes.ABC);
        CheckImpl(Shorts.None, Shorts.A, Shorts.B, Shorts.C, Shorts.AB, Shorts.BC, Shorts.AC, Shorts.ABC);
        CheckImpl(UShorts.None, UShorts.A, UShorts.B, UShorts.C, UShorts.AB, UShorts.BC, UShorts.AC, UShorts.ABC);
        CheckImpl(Ints.None, Ints.A, Ints.B, Ints.C, Ints.AB, Ints.BC, Ints.AC, Ints.ABC);
        CheckImpl(UInts.None, UInts.A, UInts.B, UInts.C, UInts.AB, UInts.BC, UInts.AC, UInts.ABC);
        CheckImpl(Longs.None, Longs.A, Longs.B, Longs.C, Longs.AB, Longs.BC, Longs.AC, Longs.ABC);
        CheckImpl(ULongs.None, ULongs.A, ULongs.B, ULongs.C, ULongs.AB, ULongs.BC, ULongs.AC, ULongs.ABC);

        static void CheckImpl<T>(
            T none,
            T a,
            T b,
            T c,
            T ab,
            T bc,
            T ac,
            T abc)
            where T : struct, Enum
        {
            none.RemoveFlags(a).ShouldBe(none);
            a.RemoveFlags(a).ShouldBe(none);

            ab.RemoveFlags(a).ShouldBe(b);
            abc.RemoveFlags(ab).ShouldBe(c);

            ab.RemoveFlags(bc).ShouldBe(a);
            ac.RemoveFlags(bc).ShouldBe(a);
        }
    }

    [Fact]
    public void NumBitsSet()
    {
        CheckImpl(Bytes.None, Bytes.A, Bytes.B, Bytes.C, Bytes.AB, Bytes.BC, Bytes.AC, Bytes.ABC);
        CheckImpl(SBytes.None, SBytes.A, SBytes.B, SBytes.C, SBytes.AB, SBytes.BC, SBytes.AC, SBytes.ABC);
        CheckImpl(Shorts.None, Shorts.A, Shorts.B, Shorts.C, Shorts.AB, Shorts.BC, Shorts.AC, Shorts.ABC);
        CheckImpl(UShorts.None, UShorts.A, UShorts.B, UShorts.C, UShorts.AB, UShorts.BC, UShorts.AC, UShorts.ABC);
        CheckImpl(Ints.None, Ints.A, Ints.B, Ints.C, Ints.AB, Ints.BC, Ints.AC, Ints.ABC);
        CheckImpl(UInts.None, UInts.A, UInts.B, UInts.C, UInts.AB, UInts.BC, UInts.AC, UInts.ABC);
        CheckImpl(Longs.None, Longs.A, Longs.B, Longs.C, Longs.AB, Longs.BC, Longs.AC, Longs.ABC);
        CheckImpl(ULongs.None, ULongs.A, ULongs.B, ULongs.C, ULongs.AB, ULongs.BC, ULongs.AC, ULongs.ABC);

        static void CheckImpl<T>(
            T none,
            T a,
            T b,
            T c,
            T ab,
            T bc,
            T ac,
            T abc)
            where T : struct, Enum
        {
            none.NumBitsSet().ShouldBe(0);
            a.NumBitsSet().ShouldBe(1);
            b.NumBitsSet().ShouldBe(1);
            c.NumBitsSet().ShouldBe(1);
            ab.NumBitsSet().ShouldBe(2);
            bc.NumBitsSet().ShouldBe(2);
            ac.NumBitsSet().ShouldBe(2);
            abc.NumBitsSet().ShouldBe(3);
        }
    }

    [Flags]
    private enum Bytes : byte
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,

        AB = A | B,
        BC = B | C,
        AC = A | C,

        ABC = A | B | C,
    }

    [Flags]
    private enum SBytes : sbyte
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,

        AB = A | B,
        BC = B | C,
        AC = A | C,

        ABC = A | B | C,
    }

    [Flags]
    private enum Shorts : short
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,

        AB = A | B,
        BC = B | C,
        AC = A | C,

        ABC = A | B | C,
    }

    [Flags]
    private enum UShorts : ushort
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,

        AB = A | B,
        BC = B | C,
        AC = A | C,

        ABC = A | B | C,
    }

    [Flags]
    private enum Ints : int
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,

        AB = A | B,
        BC = B | C,
        AC = A | C,

        ABC = A | B | C,
    }

    [Flags]
    private enum UInts : uint
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,

        AB = A | B,
        BC = B | C,
        AC = A | C,

        ABC = A | B | C,
    }

    [Flags]
    private enum Longs : long
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,

        AB = A | B,
        BC = B | C,
        AC = A | C,

        ABC = A | B | C,
    }

    [Flags]
    private enum ULongs : ulong
    {
        None = 0,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,

        AB = A | B,
        BC = B | C,
        AC = A | C,

        ABC = A | B | C,
    }
}
