using System.Collections;
using Altinn.Authorization.CommandLine.Utils;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Commands;

internal sealed class ConventionsCollection
    : ICommandConventionBuilder
    , IEnumerable<Action<ICommandBuilder>>
{
    private readonly AtomicBool _built;
    private readonly ConventionsCollection? _parent;
    private readonly List<ConventionRegistration> _conventions = new();
    private readonly List<ConventionRegistration> _finalConventions = new();

    public ConventionsCollection(AtomicBool built)
    {
        _built = built;
        _parent = null;
    }

    public ConventionsCollection(ConventionsCollection parent)
    {
        _built = parent._built;
        _parent = parent;
    }

    internal bool IsRoot
        => _parent is null;

    public IEnumerator<Action<ICommandBuilder>> GetEnumerator()
    {
        foreach (var convention in GetNormalConventions(onlyRecursive: false))
        {
            yield return convention;
        }

        foreach (var convention in GetFinalConventions(onlyRecursive: false))
        {
            yield return convention;
        }
    }

    private IEnumerable<Action<ICommandBuilder>> GetNormalConventions(bool onlyRecursive)
    {
        if (_parent is not null)
        {
            foreach (var convention in _parent.GetNormalConventions(onlyRecursive: true))
            {
                yield return convention;
            }
        }

        foreach (var convention in _conventions)
        {
            if (!onlyRecursive || convention.Recursive)
            {
                yield return convention.Convention;
            }
        }
    }

    private IEnumerable<Action<ICommandBuilder>> GetFinalConventions(bool onlyRecursive)
    {
        // final conventions are ran in the opposite order of the normal conventions, so that the last added (and most local) final convention is ran first
        foreach (var convention in _finalConventions.AsEnumerable().Reverse())
        {
            if (!onlyRecursive || convention.Recursive)
            {
                yield return convention.Convention;
            }
        }

        if (_parent is not null)
        {
            foreach (var convention in _parent.GetFinalConventions(onlyRecursive: true))
            {
                yield return convention;
            }
        }
    }

    internal ConventionsCollection CreateChild()
        => new(this);

    void ICommandConventionBuilder.Add(Action<ICommandBuilder> convention, bool recursive)
    {
        if (_built.Value)
        {
            ThrowHelper.ThrowInvalidOperationException("Collection cannot be modified after it has been built.");
        }

        _conventions.Add(new(convention, recursive));
    }

    void ICommandConventionBuilder.Finally(Action<ICommandBuilder> finallyConvention, bool recursive)
    {
        if (_built.Value)
        {
            ThrowHelper.ThrowInvalidOperationException("Collection cannot be modified after it has been built.");
        }

        _finalConventions.Add(new(finallyConvention, recursive));
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private record struct ConventionRegistration(Action<ICommandBuilder> Convention, bool Recursive);
}
