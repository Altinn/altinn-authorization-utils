using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.Sample.Api.Models;

[ExcludeFromCodeCoverage]
public class Nil
{
    public static Nil Instance { get; } = new Nil();

    private Nil()
    {
    }
}
