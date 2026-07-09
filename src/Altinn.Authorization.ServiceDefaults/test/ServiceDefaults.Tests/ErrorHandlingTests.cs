using System.Net;
using System.Text.Json;
using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.TestUtils;
using Altinn.Authorization.TestUtils.AspNetCore;
using Altinn.Authorization.TestUtils.Shouldly;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class ErrorHandlingTests
    : IAsyncLifetime
{
    private TestClient? _client;

    private TestClient Client
        => _client!;

    private static CancellationToken CancellationToken
        => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Controller_Exception_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/controller/throw-invalid", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(HttpProblemDescriptors.For(HttpStatusCode.InternalServerError).ErrorCode);
    }

    [Fact]
    public async Task Controller_ProblemException_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/controller/throw-problem", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.NotImplemented);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(TestErrors.NotImplemented.ErrorCode);
    }

    [Fact]
    public async Task Controller_AggregateProblemException_Empty_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/controller/throw-aggregate-problem/0", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(HttpProblemDescriptors.For(HttpStatusCode.InternalServerError).ErrorCode);
    }

    [Fact]
    public async Task Controller_AggregateProblemException_Single_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/controller/throw-aggregate-problem/1", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.NotImplemented);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(TestErrors.NotImplemented.ErrorCode);
    }

    [Fact]
    public async Task Controller_AggregateProblemException_Multiple_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/controller/throw-aggregate-problem/3", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);

        using var reserialized = Json.SerializeToDocument(problem);
        var multiProblem = Json.Deserialize<AltinnMultipleProblemDetails>(reserialized);

        multiProblem.ShouldNotBeNull();
        multiProblem.Problems.ShouldNotBeNull();
        multiProblem.Problems.Count.ShouldBe(3);

        foreach (var (innerProblem, index) in multiProblem.Problems.Select((p, i) => (p, i)))
        {
            innerProblem.ErrorCode.ShouldBe(TestErrors.NotImplemented.ErrorCode);
            innerProblem.Extensions.ShouldContainKey("n");
            innerProblem.Extensions["n"].ShouldBeOfType<JsonElement>()
                .GetString().ShouldBe(index.ToString());
        }
    }

    [Fact]
    public async Task Controller_Problem_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/controller/problem", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(TestErrors.InternalServerError.ErrorCode);
    }

    [Fact]
    public async Task Minimal_Exception_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/minimal/throw-invalid", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(HttpProblemDescriptors.For(HttpStatusCode.InternalServerError).ErrorCode);
    }

    [Fact]
    public async Task Minimal_ProblemException_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/minimal/throw-problem", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.NotImplemented);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(TestErrors.NotImplemented.ErrorCode);
    }

    [Fact]
    public async Task Minimal_Problem_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/minimal/problem", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(TestErrors.InternalServerError.ErrorCode);
    }

    [Fact]
    public async Task Middleware_Exception_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/middleware/throw-invalid", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.InternalServerError);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(HttpProblemDescriptors.For(HttpStatusCode.InternalServerError).ErrorCode);
    }

    [Fact]
    public async Task Middleware_ProblemException_ReturnsProblemDetails()
    {
        var response = await Client.GetAsync("/middleware/throw-problem", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.NotImplemented);

        var problem = await response.ShouldHaveJsonContent<AltinnProblemDetails>();
        problem.ErrorCode.ShouldBe(TestErrors.NotImplemented.ErrorCode);
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        IAsyncDisposable? client = Interlocked.Exchange(ref _client, null);
        if (client is not null)
        {
            return client.DisposeAsync();
        }

        return ValueTask.CompletedTask;
    }

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        _client = await TestClient.Create(
            configureBuilder: builder =>
            {
                builder.AddAltinnServiceDefaults("test");

                builder.Services.AddMvc()
                    .AddTestController<TestController>();

                builder.Services.AddAuthentication("TestClient")
                    .AddTestAuthentication("TestClient");
            },
            configureApplication: app =>
            {
                app.AddDefaultAltinnMiddleware();
                app.Use(async (context, next) =>
                {
                    await Task.Yield();

                    if (context.Request.Path == "/middleware/throw-invalid")
                    {
                        throw new InvalidOperationException("Test middleware exception");
                    }

                    if (context.Request.Path == "/middleware/throw-problem")
                    {
                        throw TestErrors.NotImplemented.Create().ToException();
                    }

                    await next(context);
                });

                app.MapDefaultAltinnEndpoints();
                app.MapGet("/minimal/throw-invalid", context =>
                {
                    throw new InvalidOperationException("Test endpoint exception");
                });
                app.MapGet("/minimal/throw-problem", context =>
                {
                    throw TestErrors.NotImplemented.Create().ToException();
                });
                app.MapGet("/minimal/problem", () =>
                {
                    return TypedResults.Problem(TestErrors.InternalServerError.ToProblemDetails(detail: "Test problem detail"));
                });
                app.MapControllers();
            });
    }

    [ApiController]
    public sealed class TestController
        : ControllerBase
    {
        [HttpGet("/controller/throw-invalid")]
        public IActionResult Throw()
        {
            throw new InvalidOperationException("Test controller exception");
        }

        [HttpGet("/controller/throw-problem")]
        public IActionResult ThrowProblem()
        {
            throw TestErrors.NotImplemented.Create().ToException();
        }

        [HttpGet("/controller/throw-aggregate-problem/{count:int}")]
        public IActionResult ThrowAggregateProblem(int count)
        {
            var problems = Enumerable.Range(0, count)
                .Select(n => TestErrors.NotImplemented.Create([new("n", n.ToString())]).ToException())
                .ToArray();

            throw new AggregateException(problems);
        }

        [HttpGet("/controller/problem")]
        public IActionResult Problem()
        {
            return TestErrors.InternalServerError.ToActionResult(detail: "Test problem detail");
        }
    }
}
