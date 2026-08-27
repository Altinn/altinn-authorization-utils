namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Provides access to the current <see cref="CommandInvocationContext"/>, if one is available.
/// </summary>
/// <remarks>
/// This interface should be used with caution. It relies on <see cref="AsyncLocal{T}" /> which can have a negative performance impact on async calls.
/// It also creates a dependency on "ambient state" which can make testing more difficult.
/// </remarks>
public interface ICommandInvocationContextAccessor
{
    /// <summary>
    /// Gets or sets the current <see cref="CommandLine.CommandInvocationContext">CommandInvocationContext</see>.
    /// Returns <see langword="null" /> if there is no active <see cref="CommandLine.CommandInvocationContext">CommandInvocationContext</see>.
    /// </summary>
    CommandInvocationContext? CommandInvocationContext { get; set; }
}
