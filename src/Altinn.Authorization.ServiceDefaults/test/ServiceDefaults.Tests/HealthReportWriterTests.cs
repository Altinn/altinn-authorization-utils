﻿using Altinn.Authorization.ServiceDefaults.HealthChecks;
using Altinn.Authorization.TestUtils.Shouldly;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;
using ExtOptions = Microsoft.Extensions.Options.Options;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class HealthReportWriterTests
{
    private static UTF8Encoding Encoding = new(throwOnInvalidBytes: true, encoderShouldEmitUTF8Identifier: false);

    [Fact]
    public async Task ContentNegotiation_DefaultsTo_PlainText()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings
        {
            Format = HealthReportWriterSettings.HealthReportFormat.Auto,
        }));
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, "self is healthy", TimeSpan.FromSeconds(2), exception: null, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        var doc = Encoding.GetString(responseStream.ToArray());
        ctx.Response.ContentType.ShouldBe(PlainTextHealthReportFormatter.ContentType);
        doc.ShouldBe("Healthy");
    }

    [Fact]
    public async Task ContentNegotiation_Supports_PlainText()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings
        {
            Format = HealthReportWriterSettings.HealthReportFormat.Auto,
        }));
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, "self is healthy", TimeSpan.FromSeconds(2), exception: null, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Request =
            {
                Headers =
                {
                    Accept = "text/plain",
                },
            },
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        var doc = Encoding.GetString(responseStream.ToArray());
        ctx.Response.ContentType.ShouldBe("text/plain");
        doc.ShouldBe("Healthy");
    }

    [Fact]
    public async Task ContentNegotiation_Supports_ApplicationJson()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings
        {
            Format = HealthReportWriterSettings.HealthReportFormat.Auto,
        }));
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, "self is healthy", TimeSpan.FromSeconds(2), exception: null, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Request =
            {
                Headers =
                {
                    Accept = "application/json",
                },
            },
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        ctx.Response.ContentType.ShouldBe("application/json");
        var doc = ParseDoc(responseStream);
        doc.ShouldBeStructurallyEquivalentTo(
            """
            {
                "status": "healthy",
                "totalDuration": "00:00:02",
                "entries": {
                    "self": {
                        "status": "healthy",
                        "description": "self is healthy",
                        "duration": "00:00:02"
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task ContentNegotiation_Supports_JsonV1ContentType()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings
        {
            Format = HealthReportWriterSettings.HealthReportFormat.Auto,
        }));
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, "self is healthy", TimeSpan.FromSeconds(2), exception: null, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Request =
            {
                Headers =
                {
                    Accept = JsonV1HealthReportFormatter.ContentType,
                },
            },
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        ctx.Response.ContentType.ShouldBe(JsonV1HealthReportFormatter.ContentType);
        var doc = ParseDoc(responseStream);
        doc.ShouldBeStructurallyEquivalentTo(
            """
            {
                "status": "healthy",
                "totalDuration": "00:00:02",
                "entries": {
                    "self": {
                        "status": "healthy",
                        "description": "self is healthy",
                        "duration": "00:00:02"
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task CanWritePlaintext()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings 
        { 
            Format = HealthReportWriterSettings.HealthReportFormat.PlainText,
        }));
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, "self is healthy", TimeSpan.FromSeconds(2), exception: null, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        var doc = Encoding.GetString(responseStream.ToArray());
        ctx.Response.ContentType.ShouldBe(PlainTextHealthReportFormatter.ContentType);
        doc.ShouldBe("Healthy");
    }

    [Fact]
    public async Task Report_SingleEntry_NoException_NoData_NoTags()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings 
        {
            Format = HealthReportWriterSettings.HealthReportFormat.JsonV1,
        }));
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, "self is healthy", TimeSpan.FromSeconds(2), exception: null, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        var doc = ParseDoc(responseStream);
        ctx.Response.ContentType.ShouldBe(JsonV1HealthReportFormatter.ContentType);
        doc.ShouldBeStructurallyEquivalentTo(
            """
            {
                "status": "healthy",
                "totalDuration": "00:00:02",
                "entries": {
                    "self": {
                        "status": "healthy",
                        "description": "self is healthy",
                        "duration": "00:00:02"
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task WriteHealthCheckReportWithoutExceptions_IgnoresExceptions()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings
        {
            Format = HealthReportWriterSettings.HealthReportFormat.JsonV1,
        }));
        var exn = new Exception("foo");
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, description: null, TimeSpan.FromSeconds(2), exception: exn, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        var doc = ParseDoc(responseStream);
        ctx.Response.ContentType.ShouldBe(JsonV1HealthReportFormatter.ContentType);
        doc.ShouldBeStructurallyEquivalentTo(
            """
            {
                "status": "healthy",
                "totalDuration": "00:00:02",
                "entries": {
                    "self": {
                        "status": "healthy",
                        "duration": "00:00:02"
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task WriteHealthCheckReportWithExceptions_UsesExceptionForDescription()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings 
        {
            Format = HealthReportWriterSettings.HealthReportFormat.JsonV1,
            Exceptions = HealthReportWriterSettings.ExceptionHandling.Include,
        }));
        var exn = new Exception("foo");
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, description: null, TimeSpan.FromSeconds(2), exception: exn, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        var doc = ParseDoc(responseStream);
        ctx.Response.ContentType.ShouldBe(JsonV1HealthReportFormatter.ContentType);
        doc.ShouldBeStructurallyEquivalentTo(
            """
            {
                "status": "healthy",
                "totalDuration": "00:00:02",
                "entries": {
                    "self": {
                        "status": "healthy",
                        "duration": "00:00:02",
                        "description": "foo",
                        "exception": {
                            "message": "foo"
                        }
                    }
                }
            }
            """);
    }

    [Fact]
    public async Task WriteHealthCheckReportWithExceptions_IncludesStackTrace()
    {
        // Arrange
        var writer = new HealthReportWriter(ExtOptions.Create(new HealthReportWriterSettings
        {
            Format = HealthReportWriterSettings.HealthReportFormat.JsonV1,
            Exceptions = HealthReportWriterSettings.ExceptionHandling.IncludeStackTrace 
                | HealthReportWriterSettings.ExceptionHandling.IncludeInnerException,
        }));

        var inner = Thrown(new Exception("inner"));
        var outer = Thrown(new Exception("outer", inner));

        var report = new HealthReport(new Dictionary<string, HealthReportEntry>
        {
            { "self", new(HealthStatus.Healthy, description: "self status", TimeSpan.FromSeconds(2), exception: outer, data: null, tags: null) }
        }, TimeSpan.FromSeconds(2));

        using var responseStream = new MemoryStream();
        var ctx = new DefaultHttpContext()
        {
            Response =
            {
                Body = responseStream,
            },
        };

        // Act
        await writer.WriteHealthCheckReport(ctx, report);

        // Assert
        var doc = ParseDoc(responseStream);
        ctx.Response.ContentType.ShouldBe(JsonV1HealthReportFormatter.ContentType);
        doc.ShouldBeStructurallyEquivalentTo(
            $$"""
            {
                "status": "healthy",
                "totalDuration": "00:00:02",
                "entries": {
                    "self": {
                        "status": "healthy",
                        "duration": "00:00:02",
                        "description": "self status",
                        "exception": {
                            "message": "outer",
                            "stackTrace": {{JsonSerializer.Serialize(outer.StackTrace)}},
                            "innerException": {
                                "message": "inner",
                                "stackTrace": {{JsonSerializer.Serialize(inner.StackTrace)}}
                            }
                        }
                    }
                }
            }
            """);
    }

    static T Thrown<T>(T exn)
        where T : Exception
    {
        try
        {
            throw exn;
        }
        catch (T e)
        {
            return e;
        }
    }

    static JsonDocument ParseDoc(MemoryStream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);
        Assert.True(ms.TryGetBuffer(out var buffer));
        return JsonDocument.Parse(buffer);
    }
}
