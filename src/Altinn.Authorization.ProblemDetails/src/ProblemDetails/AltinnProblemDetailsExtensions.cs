using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extension methods for <see cref="AltinnProblemDetails"/>,
/// <see cref="ProblemDescriptor"/>, and <see cref="ValidationErrorDescriptor"/>.
/// </summary>
public static class AltinnProblemDetailsExtensions
{
    #region ProblemDetails.ToActionResult
    /// <summary>
    /// Creates a new <see cref="ActionResult"/> that returns a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> to the client.
    /// </summary>
    /// <param name="problemDetails">The <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> to return to the client.</param>
    /// <returns>The created <see cref="ActionResult"/>.</returns>
    public static ActionResult ToActionResult(this Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
        => new ProblemDetailsActionResult(problemDetails);
    #endregion

    #region ProblemDescriptor.ToProblemDetails
    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemDescriptor descriptor)
        => new AltinnProblemDetails(descriptor);

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemDescriptor descriptor, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnProblemDetails(descriptor);
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemDescriptor descriptor, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnProblemDetails(descriptor);
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }
    #endregion

    #region ProblemDescriptor.ToActionResult
    /// <summary>
    /// Creates a new <see cref="ActionResult"/> that returns a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> to the client.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static ActionResult ToActionResult(this ProblemDescriptor descriptor)
        => descriptor.ToProblemDetails().ToActionResult();

    /// <summary>
    /// Creates a new <see cref="ActionResult"/> that returns a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> to the client.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static ActionResult ToActionResult(this ProblemDescriptor descriptor, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
        => descriptor.ToProblemDetails(extensions).ToActionResult();

    /// <summary>
    /// Creates a new <see cref="ActionResult"/> that returns a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> to the client.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static ActionResult ToActionResult(this ProblemDescriptor descriptor, IEnumerable<KeyValuePair<string, object?>> extensions)
        => descriptor.ToProblemDetails(extensions).ToActionResult();
    #endregion

    #region ValidationErrorDescriptor.ToValidationError
    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor)
        => new AltinnValidationError(descriptor);

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with a path.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="path">The path that is erroneous.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path)
        => new AltinnValidationError(descriptor)
        {
            Paths = path,
        };

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with a path.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths)
        => new AltinnValidationError(descriptor)
        {
            Paths = new StringValues([..paths]),
        };

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with a path.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => new AltinnValidationError(descriptor)
        {
            Paths = new StringValues([.. paths]),
        };

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor);
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor);
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="path">The path that is erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = path,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="path">The path that is erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = path,
        }; 
        
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = new StringValues([.. paths]),
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = new StringValues([.. paths]),
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = new StringValues([.. paths]),
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = new StringValues([.. paths]),
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }
    #endregion
}
