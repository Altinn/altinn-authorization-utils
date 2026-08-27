namespace Altinn.Authorization.CommandLine.Utils;

internal sealed class AtomicBool
{
    private byte _value;

    public AtomicBool(bool initialValue = false)
    {
        _value = initialValue ? (byte)1 : (byte)0;
    }

    public bool Value
    {
        get => Volatile.Read(ref _value) != 0;
        set => Interlocked.Exchange(ref _value, value ? (byte)1 : (byte)0);
    }

    public bool Exchange(bool newValue)
    {
        var newByte = newValue ? (byte)1 : (byte)0;
        var oldByte = Interlocked.Exchange(ref _value, newByte);

        return oldByte != 0;
    }
}
