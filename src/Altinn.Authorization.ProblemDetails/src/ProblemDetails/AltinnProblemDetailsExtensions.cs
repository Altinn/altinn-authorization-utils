using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

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

    #region ProblemInstance.ToProblemDetails
    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from a <see cref="ProblemInstance"/>.
    /// </summary>
    /// <param name="instance">The <see cref="ProblemInstance"/>.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemInstance instance)
        => CreateProblemDetails(instance);
    #endregion

    #region ProblemDescriptor.ToProblemDetails
    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemDescriptor descriptor)
        => CreateProblemDetails(descriptor);

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="detail">The error detail.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemDescriptor descriptor, string? detail)
        => CreateProblemDetails(descriptor.Create(detail));

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemDescriptor descriptor, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = CreateProblemDetails(descriptor);
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
    /// <param name="detail">The error detail.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemDescriptor descriptor, string? detail, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = CreateProblemDetails(descriptor.Create(detail));
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
        var ret = CreateProblemDetails(descriptor);
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
    /// <param name="detail">The error detail.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static AltinnProblemDetails ToProblemDetails(this ProblemDescriptor descriptor, string? detail, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = CreateProblemDetails(descriptor.Create(detail));
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }
    #endregion

    #region ProblemInstance.ToActionResult
    /// <summary>
    /// Creates a new <see cref="ActionResult"/> that returns a <see cref="ProblemInstance"/> to the client.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static ActionResult ToActionResult(this ProblemInstance instance)
        => instance.ToProblemDetails().ToActionResult();
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
    /// <param name="detail">The error detail.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static ActionResult ToActionResult(this ProblemDescriptor descriptor, string? detail)
        => descriptor.ToProblemDetails(detail).ToActionResult();

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
    /// <param name="detail">The error detail.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static ActionResult ToActionResult(this ProblemDescriptor descriptor, string? detail, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
        => descriptor.ToProblemDetails(detail, extensions).ToActionResult();

    /// <summary>
    /// Creates a new <see cref="ActionResult"/> that returns a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> to the client.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static ActionResult ToActionResult(this ProblemDescriptor descriptor, IEnumerable<KeyValuePair<string, object?>> extensions)
        => descriptor.ToProblemDetails(extensions).ToActionResult();

    /// <summary>
    /// Creates a new <see cref="ActionResult"/> that returns a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> to the client.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="detail">The error detail.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnProblemDetails"/>.</returns>
    public static ActionResult ToActionResult(this ProblemDescriptor descriptor, string? detail, IEnumerable<KeyValuePair<string, object?>> extensions)
        => descriptor.ToProblemDetails(detail, extensions).ToActionResult();
    #endregion

    #region ValidationErrorInstance.ToValidationError
    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorInstance instance)
        => new AltinnValidationError(instance);
    #endregion

    #region ValidationErrorDescriptor.ToValidationError
    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor.
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
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path)
        => new AltinnValidationError(descriptor)
        {
            Paths = [path],
        };

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with a path.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="path">The path that is erroneous.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path, string? detail)
        => new AltinnValidationError(descriptor)
        {
            Paths = [path],
            Detail = detail,
        };

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with a path.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths)
        => new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
        };

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with a path.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, string? detail)
        => new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
            Detail = detail,
        };

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with a path.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
        };

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with a path.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, string? detail)
        => new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
            Detail = detail,
        };

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
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
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <param name="detail">The error detail.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<KeyValuePair<string, object?>> extensions, string? detail)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Detail = detail,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
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
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <param name="detail">The error detail.</param>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<KeyValuePair<string, object?>> extensions, string? detail)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Detail = detail,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="path">The path that is erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [path],
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="path">The path that is erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path, ReadOnlySpan<KeyValuePair<string, object?>> extensions, string? detail)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [path],
            Detail = detail,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="path">The path that is erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [path],
        }; 
        
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="path">The path that is erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, string path, IEnumerable<KeyValuePair<string, object?>> extensions, string? detail)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [path],
            Detail = detail,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, ReadOnlySpan<KeyValuePair<string, object?>> extensions, string? detail)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
            Detail = detail,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths, IEnumerable<KeyValuePair<string, object?>> extensions, string? detail)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
            Detail = detail,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, ReadOnlySpan<KeyValuePair<string, object?>> extensions, string? detail)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
            Detail = detail,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnValidationError"/> from this descriptor with additional properties.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <param name="detail">The error detail.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths, IEnumerable<KeyValuePair<string, object?>> extensions, string? detail)
    {
        var ret = new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
            Detail = detail,
        };

        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }
    #endregion

    #region ValidationErrorBuilder.TryToProblemDetails
    /// <summary>
    /// Tries to convert the validation errors to a <see cref="AltinnValidationProblemDetails"/>.
    /// </summary>
    /// <param name="errors">This <see cref="ValidationErrorBuilder"/> instance.</param>
    /// <param name="result">The resulting <see cref="AltinnValidationProblemDetails"/>, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="errors"/> was not empty and a <see cref="AltinnValidationProblemDetails"/> was created,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryToProblemDetails(this ref ValidationErrorBuilder errors, [NotNullWhen(true)] out AltinnValidationProblemDetails? result)
    {
        if (errors.TryBuild(out var instance))
        {
            result = new AltinnValidationProblemDetails(instance);
            return true;
        }

        result = null;
        return false;
    }
    #endregion

    #region ValidationErrorBuilder.TryToActionResult
    /// <summary>
    /// Tries to convert the validation errors to a <see cref="ActionResult"/>.
    /// </summary>
    /// <param name="errors">This <see cref="ValidationErrorBuilder"/> instance.</param>
    /// <param name="result">The resulting <see cref="ActionResult"/>, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="errors"/> was not empty and a <see cref="ActionResult"/> was created,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryToActionResult(this ref ValidationErrorBuilder errors, [NotNullWhen(true)] out ActionResult? result)
    {
        if (errors.TryToProblemDetails(out var details))
        {
            result = details.ToActionResult();
            return true;
        }

        result = null;
        return false;
    }
    #endregion

    #region MultipleProblemBuilder.TryToProblemDetails
    /// <summary>
    /// Tries to convert the problems to a <see cref="AltinnProblemDetails"/>.
    /// </summary>
    /// <param name="errors">This <see cref="MultipleProblemBuilder"/> instance.</param>
    /// <param name="result">The resulting <see cref="AltinnProblemDetails"/>, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="errors"/> was not empty and a <see cref="AltinnProblemDetails"/> was created,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryToProblemDetails(this ref MultipleProblemBuilder errors, [NotNullWhen(true)] out AltinnProblemDetails? result)
    {
        if (errors.TryBuild(out var instance))
        {
            result = instance.ToProblemDetails();
            return true;
        }

        result = null;
        return false;
    }
    #endregion

    #region MultipleProblemBuilder.TryToActionResult
    /// <summary>
    /// Tries to convert the problems to a <see cref="ActionResult"/>.
    /// </summary>
    /// <param name="errors">This <see cref="MultipleProblemBuilder"/> instance.</param>
    /// <param name="result">The resulting <see cref="ActionResult"/>, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="errors"/> was not empty and a <see cref="ActionResult"/> was created,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryToActionResult(this ref MultipleProblemBuilder errors, [NotNullWhen(true)] out ActionResult? result)
    {
        if (errors.TryToProblemDetails(out var details))
        {
            result = details.ToActionResult();
            return true;
        }

        result = null;
        return false;
    }
    #endregion

    private static AltinnProblemDetails CreateProblemDetails(ProblemInstance instance)
        => instance switch
        {
            ValidationProblemInstance validationProblem => CreateProblemDetails(validationProblem),
            MultipleProblemInstance multipleProblem => CreateProblemDetails(multipleProblem),
            _ => new AltinnProblemDetails(instance),
        };

    private static AltinnValidationProblemDetails CreateProblemDetails(ValidationProblemInstance instance)
    {
        return new AltinnValidationProblemDetails(instance);
    }

    private static AltinnMultipleProblemDetails CreateProblemDetails(MultipleProblemInstance instance)
    {
        return new AltinnMultipleProblemDetails(instance);
    }
}
