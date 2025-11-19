using Altinn.Authorization.ServiceDefaults;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

var builder = AltinnHost.CreateWebApplicationBuilder("test", args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerAutoXmlDoc();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorizationModelUtilsSwaggerSupport();
builder.Services.AddSwaggerFilterAttributeSupport();
builder.Services.AddSwaggeAltinnSecuritySupport();
builder.Services.AddSwaggerAltinnServers(c =>
{
    c.IncludePerformanceTestServer = false;        
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("policy:read", policy => policy.RequireScopeAnyOf("read", "write", "admin"))
    .AddPolicy("policy:write", policy => policy.RequireScopeAnyOf("write", "admin"))
    .AddPolicy("policy:admin", policy => policy.RequireScopeAnyOf("admin"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseReDoc();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

[ExcludeFromCodeCoverage]
partial class Program
{
}
