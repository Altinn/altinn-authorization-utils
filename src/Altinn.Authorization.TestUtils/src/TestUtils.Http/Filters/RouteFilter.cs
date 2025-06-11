using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.TestUtils.Http.Filters;

internal sealed class RouteFilter
    : IFakeRequestFilter
{
    public static IFakeRequestFilter Create([StringSyntax("Route")] string routeTemplate)
        => Create(TemplateParser.Parse(routeTemplate));

    public static IFakeRequestFilter Create(RouteTemplate routeTemplate)
        => new RouteFilter(routeTemplate);

    private readonly TemplateMatcher _matcher;

    private RouteFilter(RouteTemplate template)
    {
        _matcher = new(template, new());
    }

    public string Description => $"has path '{_matcher.Template.TemplateText}'";

    public bool Matches(FakeHttpRequestMessage request)
    {
        if (request.RequestUri is null)
        {
            return false;
        }

        var baseUrl = FakeHttpMessageHandler.FakeBasePath;
        var relativeUrl = baseUrl.MakeRelativeUri(request.RequestUri);
        if (relativeUrl.IsAbsoluteUri)
        {
            return false;
        }

        var relStr = $"/{relativeUrl}";
        if (relStr.StartsWith("/../"))
        {
            return false;
        }

        if (relStr.IndexOf('?') is var queryStart and >= 0)
        {
            relStr = relStr[..queryStart];
        }

        RouteData routeData = request.RouteData is null ? new() : new(request.RouteData);
        if (!_matcher.TryMatch(relStr, routeData.Values))
        {
            return false;
        }

        request.RouteData = routeData;
        return true;
    }
}
