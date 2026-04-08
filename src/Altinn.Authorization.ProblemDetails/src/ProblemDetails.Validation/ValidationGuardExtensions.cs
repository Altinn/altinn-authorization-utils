using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails.Validation;

[Embedded]
internal static class ValidationGuardExtensions
{
    extension(Guard)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotEmpty<T>(
            ImmutableArray<T> value,
            [CallerArgumentExpression(nameof(value))] string name = "")
        {
            if (value.IsDefaultOrEmpty)
            {
                ThrowHelper.ThrowArgumentExceptionForIsNotEmptyWithImmutableArray<T>(name);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsValidRootPath(
            string value,
            [CallerArgumentExpression(nameof(value))] string name = "")
        {
            Guard.IsNotNull(value, name);

            if (!value.StartsWith('/'))
            {
                ThrowHelper.ThrowArgumentExceptionForIsValidRootPath(value, name);
            }

            if (value.Length > 1 && value.EndsWith('/'))
            {
                ThrowHelper.ThrowArgumentExceptionForIsValidRootPath(value, name);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsValidChildPath(
            string value,
            [CallerArgumentExpression(nameof(value))] string name = "")
        {
            Guard.IsNotNullOrEmpty(value, name);

            if (!value.StartsWith('/'))
            {
                ThrowHelper.ThrowArgumentExceptionForIsValidChildPath(value, name);
            }

            if (value.EndsWith('/'))
            {
                ThrowHelper.ThrowArgumentExceptionForIsValidChildPath(value, name);
            }
        }
    }

    [StackTraceHidden]
    private static class ThrowHelper
    {
        /// <summary>
        /// Returns a formatted representation of the input value.
        /// </summary>
        /// <param name="obj">The input <see cref="object"/> to format.</param>
        /// <returns>A formatted representation of <paramref name="obj"/> to display in error messages.</returns>
        private static string AssertString(object? obj)
        {
            return obj switch
            {
                string _ => $"\"{obj}\"",
                null => "null",
                _ => $"<{obj}>"
            };
        }

        [DoesNotReturn]
        public static void ThrowArgumentExceptionForIsNotEmptyWithImmutableArray<T>(string name)
        {
            throw new ArgumentException($"Parameter {AssertString(name)} ({typeof(ImmutableArray<T>).ToTypeString()}) must not be empty.", name);
        }

        [DoesNotReturn]
        public static void ThrowArgumentExceptionForIsValidRootPath(string value, string name)
        {
            throw new ArgumentException($"Parameter {AssertString(name)} (string) must be a valid root path, but was {AssertString(value)}.", name);
        }

        [DoesNotReturn]
        public static void ThrowArgumentExceptionForIsValidChildPath(string value, string name)
        {
            throw new ArgumentException($"Parameter {AssertString(name)} (string) must be a valid child path, but was {AssertString(value)}.", name);
        }
    }
}
