using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.Authorization.RepoCtl.Model;

internal static class AltinnVerticalDependencyResolver
{
    private static readonly FrozenDictionary<AltinnVerticalKind, AltinnVerticalKindSet> _validDependencies
        = CreateValidDependenciesDictionary();

    private static FrozenDictionary<AltinnVerticalKind, AltinnVerticalKindSet> CreateValidDependenciesDictionary()
    {
        AltinnVerticalKindSet onlyPackage = [AltinnVerticalKind.Package];
        AltinnVerticalKindSet libraryAndPackage = [AltinnVerticalKind.Library, AltinnVerticalKind.Package];

        var builder = new Dictionary<AltinnVerticalKind, AltinnVerticalKindSet>(capacity: 4)
        {
            [AltinnVerticalKind.Library] = libraryAndPackage,
            [AltinnVerticalKind.Package] = onlyPackage,
            [AltinnVerticalKind.Tool] = libraryAndPackage,
            [AltinnVerticalKind.Application] = libraryAndPackage,
        };

        return builder.ToFrozenDictionary();
    }

    public static Result ResolveDependencies(AltinnVerticalSet verticals)
    {
        MultipleProblemBuilder problems = default;

        // 1. solve direct dependencies
        foreach (var vertical in verticals)
        {
            var directDependencies = ImmutableArray.CreateBuilder<AltinnVertical>(vertical.Config.Dependencies.Count);
            var directDevDependencies = ImmutableArray.CreateBuilder<AltinnVertical>(vertical.Config.DevDependencies.Count);

            foreach (var dependencyCfg in vertical.Config.Dependencies
                .Select(id => new { Id = id, IsDevDependency = false })
                .Concat(vertical.Config.DevDependencies.Select(id => new { Id = id, IsDevDependency = true })))
            {
                if (!verticals.TryGet(dependencyCfg.Id, out var dependency))
                {
                    problems.Add(Problems.DependencyNotFound, detail: $"The vertical '{vertical.Id}' has a dependency on '{dependencyCfg.Id}', which was not found in the repository.");
                    continue;
                }

                if (!IsValidDependency(vertical.Id, dependency.Id))
                {
                    problems.Add(Problems.InvalidDependency, detail: $"The vertical '{vertical.Id}' has a dependency on '{dependencyCfg.Id}', which is not a valid dependency according to the vertical kinds.");
                }

                if (dependencyCfg.IsDevDependency)
                {
                    directDevDependencies.Add(dependency);
                }
                else
                {
                    directDependencies.Add(dependency);
                }
            }

            vertical.DirectDependencies = AltinnVerticalSet.Create(directDependencies);
            vertical.DirectDevDependencies = AltinnVerticalSet.Create(directDevDependencies);
        }

        // 2. solve full dependency graph
        ResolveAllDependencies(verticals, ref problems);
        ResolveAllDependents(verticals);

        if (problems.TryBuild(out var problem))
        {
            return problem;
        }

        return Result.Success;
    }

    private static bool IsValidDependency(AltinnVerticalId dependent, AltinnVerticalId dependency)
    {
        if (!_validDependencies.TryGetValue(dependent.Kind, out var validDependencyKinds))
        {
            throw new UnreachableException($"The vertical kind '{dependent.Kind}' was not added to the valid dependencies dictionary. This is a bug in the dependency resolver.");
        }

        return validDependencyKinds.Contains(dependency.Kind);
    }

    private static void ResolveAllDependents(AltinnVerticalSet verticals)
    {
        var builders = verticals.AsEnumerable().ToDictionary(
            static v => v.Id,
            static _ => ImmutableArray.CreateBuilder<AltinnVertical>());

        foreach (var dependent in verticals)
        {
            foreach (var dependency in dependent.AllDependencies)
            {
                builders[dependency.Id].Add(dependent);
            }
        }

        foreach (var vertical in verticals)
        {
            vertical.Dependents = AltinnVerticalSet.Create(builders[vertical.Id]);
        }
    }

    private static void ResolveAllDependencies(AltinnVerticalSet verticals, ref MultipleProblemBuilder problems)
    {
        var active = new List<AltinnVerticalId>();
        var stack = new Stack<Frame>();

        foreach (var vertical in verticals)
        {
            if (vertical.IsResolved)
            {
                continue;
            }

            stack.Push(new Frame(vertical));
            active.Add(vertical.Id);

            while (stack.TryPeek(out var frame))
            {
                if (!frame.MoveNext())
                {
                    Debug.Assert(((IEnumerable<AltinnVertical>)frame.Vertical.DirectDependencies).All(d => d.IsResolved));
                    Debug.Assert(((IEnumerable<AltinnVertical>)frame.Vertical.DirectDevDependencies).All(d => d.IsResolved));

                    // All direct dependencies of the current vertical have been processed
                    var all = ImmutableArray.CreateBuilder<AltinnVertical>(frame.Vertical.DirectDependencies.Count);
                    var allBuild = ImmutableArray.CreateBuilder<AltinnVertical>(frame.Vertical.DirectDependencies.Count + frame.Vertical.DirectDevDependencies.Count);
                    foreach (var dep in frame.Vertical.DirectDependencies)
                    {
                        all.Add(dep);
                        allBuild.Add(dep);

                        foreach (var transitiveDep in dep.AllDependencies)
                        {
                            if (!all.Contains(transitiveDep))
                            {
                                all.Add(transitiveDep);
                                allBuild.Add(transitiveDep);
                            }
                        }
                    }

                    foreach (var dep in frame.Vertical.DirectDevDependencies)
                    {
                        if (!allBuild.Contains(dep))
                        {
                            allBuild.Add(dep);

                            foreach (var transitiveDep in dep.AllDependencies)
                            {
                                if (!all.Contains(transitiveDep))
                                {
                                    allBuild.Add(transitiveDep);
                                }
                            }
                        }
                    }

                    frame.Vertical.AllDependencies = AltinnVerticalSet.Create(all);
                    frame.Vertical.AllBuildDependencies = AltinnVerticalSet.Create(allBuild);
                    Debug.Assert(active[^1] == frame.Vertical.Id);
                    active.RemoveAt(active.Count - 1);
                    stack.Pop();
                    continue;
                }

                var dependency = frame.Current;
                if (dependency.IsResolved)
                {
                    continue;
                }

                if (active.Contains(dependency.Id))
                {
                    var cycleStart = active.IndexOf(dependency.Id);
                    var cycle = string.Join(" -> ", active.Skip(cycleStart).Append(dependency.Id));
                    problems.Add(Problems.DependencyCycle, detail: $"A dependency cycle was detected: {cycle}");
                    return;
                }

                stack.Push(new Frame(dependency));
                active.Add(dependency.Id);
            }
        }

        Debug.Assert(active.Count == 0);
        Debug.Assert(((IEnumerable<AltinnVertical>)verticals).All(v => v.IsResolved));
    }

    private sealed class Frame(AltinnVertical vertical)
    {
        private bool _regularDepsCompleted;
        private ImmutableArray<AltinnVertical>.Enumerator _depsEnumerator
            = vertical.DirectDependencies.GetEnumerator();

        private ImmutableArray<AltinnVertical>.Enumerator _devDepsEnumerator
            = vertical.DirectDevDependencies.GetEnumerator();

        public AltinnVertical Vertical { get; } = vertical;

        public bool MoveNext()
        {
            if (!_regularDepsCompleted)
            {
                if (_depsEnumerator.MoveNext())
                {
                    return true;
                }

                _regularDepsCompleted = true;
            }

            return _devDepsEnumerator.MoveNext();
        }

        public AltinnVertical Current
            => _regularDepsCompleted ? _devDepsEnumerator.Current : _depsEnumerator.Current;
    }
}
