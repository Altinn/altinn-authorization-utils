namespace Altinn.Authorization.CommandLine.Tests.AsyncState;

public sealed record Thing
    : IThing
{
    public string Hello() => "Hello, World!";
}

public interface IThing
{
    public string Hello();
}
