using System.Collections.Immutable;
using System.Text;

namespace Altinn.Authorization.TestUtils.Http;

/// <summary>
/// Represents the result of attempting to match a request in a fake or test context, indicating whether the match was
/// successful and, if not, providing reasons for the mismatch.
/// </summary>
/// <remarks>This class is typically used in testing scenarios to capture the outcome of request matching logic. A
/// successful match is represented by the static Success property. If the match fails, the instance contains one or
/// more reasons describing why the match did not succeed. Instances of this class are immutable.</remarks>
public class FakeRequestMatchResult
{
    /// <summary>
    /// Gets a result that represents a successful fake request match with no errors.
    /// </summary>
    public static FakeRequestMatchResult Success { get; } = new([]);

    private readonly ImmutableList<string> _missmatchReasons;

    private FakeRequestMatchResult(ImmutableList<string> missmatchReasons)
    {
        _missmatchReasons = missmatchReasons;
    }

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess => _missmatchReasons.IsEmpty;

    /// <summary>
    /// Gets the collection of reasons explaining why a mismatch occurred.
    /// </summary>
    public IEnumerable<string> MissmatchReasons => _missmatchReasons;

    /// <inheritdoc/>
    public override string ToString()
    {
        if (IsSuccess)
        {
            return "Success";
        }

        if (_missmatchReasons.Count == 1)
        {
            return _missmatchReasons[0];
        }

        var sb = new StringBuilder();
        sb.Append("Multiple reasons for mismatch:");
        
        foreach (var reason in _missmatchReasons)
        {
            sb.AppendLine();
            sb.Append(" - ");
            sb.Append(reason);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Combines two <see cref="FakeRequestMatchResult"/> instances, returning a new instance that contains
    /// the combination of mismatch reasons from both inputs.
    /// </summary>
    /// <returns>A <see cref="FakeRequestMatchResult"/> that is successful if both input is successful; otherwise, a result
    /// containing the combined mismatch reasons from both inputs.</returns>
    public static FakeRequestMatchResult operator +(FakeRequestMatchResult left, FakeRequestMatchResult right)
    {
        if (left.IsSuccess)
        {
            return right;
        }

        if (right.IsSuccess)
        {
            return left;
        }

        return new(left._missmatchReasons.AddRange(right._missmatchReasons));
    }

    /// <summary>
    /// Creates a new <see cref="FakeRequestMatchResult"/> by adding a specified mismatch reason to the existing result.
    /// </summary>
    /// <param name="left">The existing <see cref="FakeRequestMatchResult"/> to which the mismatch reason will be added.</param>
    /// <param name="reason">The reason to add to the mismatch reasons collection. Cannot be null.</param>
    /// <returns>A new <see cref="FakeRequestMatchResult"/> that includes the specified mismatch reason in addition to those in the original
    /// result.</returns>
    public static FakeRequestMatchResult operator +(FakeRequestMatchResult left, string reason)
    {
        return new(left._missmatchReasons.Add(reason));
    }
}
