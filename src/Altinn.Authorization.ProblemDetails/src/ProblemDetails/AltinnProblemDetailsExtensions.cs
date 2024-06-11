﻿using Microsoft.AspNetCore.Mvc;
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
        => new AltinnProblemDetails(instance);
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
    /// <param name="paths">The paths that are erroneous.</param>
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
    /// <returns>A <see cref="AltinnValidationError"/>.</returns>
    public static AltinnValidationError ToValidationError(this ValidationErrorDescriptor descriptor, IEnumerable<string> paths)
        => new AltinnValidationError(descriptor)
        {
            Paths = [.. paths],
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
    /// <param name="path">The path that is erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
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
    /// <param name="paths">The paths that are erroneous.</param>
    /// <param name="extensions">Additional properties for the error.</param>
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
    #endregion

    #region ValidationErrors.TryToProblemDetails
    /// <summary>
    /// Tries to convert the validation errors to a <see cref="AltinnValidationProblemDetails"/>.
    /// </summary>
    /// <param name="errors">This <see cref="ValidationErrors"/> instance.</param>
    /// <param name="result">The resulting <see cref="AltinnValidationProblemDetails"/>, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="errors"/> was not empty and a <see cref="AltinnValidationProblemDetails"/> was created,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryToProblemDetails(this ref ValidationErrors errors, [NotNullWhen(true)] out AltinnValidationProblemDetails? result)
    {
        if (errors.IsEmpty)
        {
            result = null;
            return false;
        }

        var immutableList = errors.MapToImmutable(static v => v.ToValidationError());
        result = new AltinnValidationProblemDetails(immutableList);
        return true;
    }
    #endregion

    #region ValidationErrors.TryToActionResult
    /// <summary>
    /// Tries to convert the validation errors to a <see cref="ActionResult"/>.
    /// </summary>
    /// <param name="errors">This <see cref="ValidationErrors"/> instance.</param>
    /// <param name="result">The resulting <see cref="ActionResult"/>, or <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="errors"/> was not empty and a <see cref="ActionResult"/> was created,
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryToActionResult(this ref ValidationErrors errors, [NotNullWhen(true)] out ActionResult? result)
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
}
