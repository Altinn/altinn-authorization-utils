using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddAuthorizationModelUtilsBinders()
    .AddJsonOptions(options =>
    {
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerAutoXmlDoc();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorizationModelUtilsSwaggerSupport();
builder.Services.AddSwaggerFilterAttributeSupport();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program
{
}
