namespace Altinn.Authorization.TestUtils.Http.Filters;

internal sealed class ConstAuthenticationFilter
    : IFakeRequestFilter
{
    public static IFakeRequestFilter Create(string scheme, string parameter)
        => new ConstAuthenticationFilter(scheme, parameter);

    private readonly string _scheme;
    private readonly string _parameter;

    private ConstAuthenticationFilter(string scheme, string parameter)
    {
        _scheme = scheme;
        _parameter = parameter;
    }

    public string Description => $"has authentication header with scheme '{_scheme}' and parameter '{_parameter}'";

    public bool Matches(FakeHttpRequestMessage request)
    {
        var auth = request.Headers.Authorization;
        if (auth is null)
        {
            return false;
        }

        return auth.Scheme == _scheme && auth.Parameter == _parameter;
    }
}
