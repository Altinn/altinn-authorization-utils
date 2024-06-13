using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql.Tests;

internal static class ConfigurationBuilderExtensions
{
    public static void AddJsonConfiguration(
         this IConfigurationBuilder builder,
         [StringSyntax(StringSyntaxAttribute.Json)] string json)
    {
        var data = Encoding.UTF8.GetBytes(json);

        builder.Add(new JsonBlobConfigurationSource(data));
    }

    private class JsonBlobConfigurationSource
        : IConfigurationSource
    {
        private readonly byte[] _data;

        public JsonBlobConfigurationSource(
            byte[] data)
        {
            _data = data;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new JsonStreamConfigurationProvider(
                new JsonStreamConfigurationSource()
                {
                    Stream = new MemoryStream(_data),
                });
        }
    }
}
