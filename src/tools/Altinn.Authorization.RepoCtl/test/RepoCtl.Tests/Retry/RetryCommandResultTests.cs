using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.Retry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace Altinn.Authorization.RepoCtl.Tests.Retry;

public class RetryCommandResultTests
{
    [Fact]
    public async Task Execute_SuccessfulFirstAttempt_DoesNotRetry()
    {
        var timeProvider = new FakeTimeProvider();
        var attempts = 0;
        var delayCalls = 0;
        var inner = new CallbackCommandResult(context =>
        {
            attempts++;
            context.ReturnCode.ShouldBe(0);
        });

        var result = await RunAsync(
            inner.Retry(3, _ =>
            {
                delayCalls++;
                return TimeSpan.FromDays(1);
            }),
            timeProvider,
            TestContext.Current.CancellationToken);

        result.ShouldBe(0);
        attempts.ShouldBe(1);
        delayCalls.ShouldBe(0);
    }

    [Fact]
    public async Task Execute_EventuallySuccessful_RetriesUntilSuccess()
    {
        var timeProvider = new FakeTimeProvider();
        var attempts = 0;
        var delayAttempts = new List<int>();
        var inner = new CallbackCommandResult(context =>
        {
            attempts++;
            context.ReturnCode.ShouldBe(0);
            context.ReturnCode = attempts < 3 ? attempts : 0;
        });

        var invocation = RunAsync(
            inner.Retry(5, attempt =>
            {
                delayAttempts.Add(attempt);
                return TimeSpan.FromDays(1);
            }),
            timeProvider,
            TestContext.Current.CancellationToken);

        var result = await CompleteWithVirtualTimeAsync(invocation, timeProvider, TestContext.Current.CancellationToken);

        result.ShouldBe(0);
        attempts.ShouldBe(3);
        delayAttempts.ShouldBe([1, 2]);
    }

    [Fact]
    public async Task Execute_AllAttemptsFail_PropagatesLastReturnCode()
    {
        var timeProvider = new FakeTimeProvider();
        var attempts = 0;
        var delayAttempts = new List<int>();
        var inner = new CallbackCommandResult(context =>
        {
            attempts++;
            context.ReturnCode.ShouldBe(0);
            context.ReturnCode = attempts;
        });

        var invocation = RunAsync(
            inner.Retry(3, attempt =>
            {
                delayAttempts.Add(attempt);
                return TimeSpan.FromDays(1);
            }),
            timeProvider,
            TestContext.Current.CancellationToken);

        var result = await CompleteWithVirtualTimeAsync(invocation, timeProvider, TestContext.Current.CancellationToken);

        result.ShouldBe(3);
        attempts.ShouldBe(3);
        delayAttempts.ShouldBe([1, 2]);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(3, 8)]
    public void DefaultDelay_UsesExponentialBackoff(int attempt, int expectedSeconds)
    {
        RetryCommandResult.DefaultDelay(attempt).ShouldBe(TimeSpan.FromSeconds(expectedSeconds));
    }

    private static async Task<int> RunAsync(
        ICommandResult result,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default)
    {
        var builder = CliApplication.CreateBuilder("RetryCommandResult tests");
        builder.Services.Replace(ServiceDescriptor.Singleton(timeProvider));

        await using var application = builder.Build();
        application.AddCommand("run", "Run the test command", () => result);

        return await application.RunAsync(["run"], cancellationToken);
    }

    private static async Task<int> CompleteWithVirtualTimeAsync(
        Task<int> invocation,
        FakeTimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < 20 && !invocation.IsCompleted; i++)
        {
            await Task.Yield();
            timeProvider.Advance(TimeSpan.FromDays(1));
        }

        return await invocation.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
    }

    private sealed class CallbackCommandResult(Action<CommandInvocationContext> callback)
        : ICommandResult
    {
        public Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            callback(context);
            return Task.CompletedTask;
        }
    }
}
