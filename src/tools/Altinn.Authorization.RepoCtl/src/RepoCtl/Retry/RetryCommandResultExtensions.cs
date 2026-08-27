using Altinn.Authorization.CommandLine.Results;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Retry;

internal static class RetryCommandResultExtensions
{
    private static readonly Func<int, TimeSpan> DefaultDelayFactory = static attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt));

    extension(RetryCommandResult)
    {
        public static TimeSpan DefaultDelay(int attempt)
            => DefaultDelayFactory(attempt);
    }

    extension(ICommandResult result)
    {
        public ICommandResult Retry(ushort maxRetries, Func<int, TimeSpan>? delayFactory = null)
        {
            Guard.IsNotNull(result);
            Guard.IsGreaterThan(maxRetries, 0U);

            return new RetryCommandResult(result, maxRetries, delayFactory ?? DefaultDelayFactory);
        }
    }
}
