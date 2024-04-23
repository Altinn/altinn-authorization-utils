namespace Altinn.Urn.Sample.Api.Models;

public class Nil
{
    public static Nil Instance { get; } = new Nil();

    private Nil()
    {
    }
}
