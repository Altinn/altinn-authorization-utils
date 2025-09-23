using Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class ServiceDefaultsHttpClientDependencyInjectionExtensionsTests
{
    [Fact]
    public void ConfigureBaseAddress_Uri()
    {
        Uri baseAddress = new("https://example.com/api/");
        var services = new ServiceCollection();
        services.AddHttpClient("test")
            .ConfigureBaseAddress(baseAddress);

        using var s = services.BuildServiceProvider();
        var factory = s.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient("test");

        client.BaseAddress.ShouldBe(baseAddress);
    }

    [Fact]
    public void ConfigureBaseAddress_String()
    {
        string baseAddress = "https://example.com/api/";
        var services = new ServiceCollection();
        services.AddHttpClient("test")
            .ConfigureBaseAddress(baseAddress);

        using var s = services.BuildServiceProvider();
        var factory = s.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient("test");

        client.BaseAddress.ShouldBe(new(baseAddress));
    }

    [Fact]
    public void ConfigureBaseAddress_FromOptions_Uri()
    {
        var services = new ServiceCollection();
        services.AddOptions<TestOptionsUri>();
        services.AddHttpClient("test")
            .ConfigureBaseAddressFromOptions<TestOptionsUri>(o => o.BaseAddress);

        using var s = services.BuildServiceProvider();
        var factory = s.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient("test");

        var options = s.GetRequiredService<IOptions<TestOptionsUri>>().Value;

        client.BaseAddress.ShouldBe(options.BaseAddress);
    }

    [Fact]
    public void ConfigureBaseAddress_FromOptions_String()
    {
        var services = new ServiceCollection();
        services.AddOptions<TestOptionsString>();
        services.AddHttpClient("test")
            .ConfigureBaseAddressFromOptions<TestOptionsString>(o => o.BaseAddress);

        using var s = services.BuildServiceProvider();
        var factory = s.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient("test");

        var options = s.GetRequiredService<IOptions<TestOptionsString>>().Value;

        client.BaseAddress.ShouldBe(new(options.BaseAddress));
    }

    [Fact]
    public void TryAddPlatformAccessTokenProvider_AddsIfNoneRegistered()
    {
        var services = new ServiceCollection();
        
        services.TryAddPlatformAccessTokenProvider<TestPlatformAccessTokenProvider>();
        services.Count.ShouldBe(1);

        services.TryAddPlatformAccessTokenProvider<TestPlatformAccessTokenProvider>();
        services.Count.ShouldBe(1);

        services.TryAddPlatformAccessTokenProvider<TestPlatformAccessTokenProvider2>();
        services.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddPlatformAccessTokenHandler()
    {
        var services = new ServiceCollection();
        services.TryAddPlatformAccessTokenProvider<TestPlatformAccessTokenProvider>();

        services.AddHttpClient("test")
            .ConfigureBaseAddress("https://example.com/api/")
            .AddPlatformAccessTokenHandler();

        using var s = services.BuildServiceProvider();
        var factory = s.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient("test");

        var ex = await Should.ThrowAsync<NotImplementedException>(async () => await client.GetAsync("resource"));
        ex.Message.ShouldBe(nameof(TestPlatformAccessTokenProvider));
    }

    private class TestOptionsUri
    {
        public Uri BaseAddress { get; set; } = new("https://example.com/api/");
    }

    private class TestOptionsString
    {
        public string BaseAddress { get; set; } = "https://example.com/api/";
    }

    private class TestPlatformAccessTokenProvider
        : IPlatformAccessTokenProvider
    {
        public ValueTask<string> GetPlatformAccessToken(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(nameof(TestPlatformAccessTokenProvider));
        }
    }

    private class TestPlatformAccessTokenProvider2
        : IPlatformAccessTokenProvider
    {
        public ValueTask<string> GetPlatformAccessToken(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(nameof(TestPlatformAccessTokenProvider2)); 
        }
    }
}
