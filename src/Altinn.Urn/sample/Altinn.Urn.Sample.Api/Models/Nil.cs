namespace Altinn.Urn.Sample.Api.Models;

[ExcludeFromDescription]
public class Nil
{
    public static Nil Instance { get; } = new Nil();

    private Nil()
    {
    }
}
