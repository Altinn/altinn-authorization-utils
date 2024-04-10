using Microsoft.CodeAnalysis;
using System.Collections;
using System.Text;

namespace Altinn.Urn.SourceGenerator.Emitting;

internal class PrefixTree<T>
{
    private readonly Node _root = new Node() { PathLength = 0 };

    public void Add(string key, T value)
    {
        Node current = _root;
        foreach (char c in key)
        {
            if (!current.Children.TryGetValue(c, out Node? next))
            {
                next = new Node() { PathLength = current.PathLength + 1 };
                current.Children.Add(c, next);
            }

            current = next;
        }

        if (current.Value.HasValue)
        {
            throw new InvalidOperationException("Key already exists");
        }

        current.Value = value;
    }

    public IPrefixNode<T> Flatten() => Flatten(string.Empty, _root);

    private static FlattenedNode Flatten(string prefix, Node node)
    {
        var sb = new StringBuilder();
        var children = new List<FlattenedNode>();

        foreach (var kvp in node.Children)
        {
            sb.Clear();
            sb.Append(kvp.Key);

            var child = kvp.Value;
            while (child.Children.Count == 1 && !child.Value.HasValue)
            {
                var next = child.Children.Single();
                sb.Append(next.Key);
                child = next.Value;
            }

            children.Add(Flatten(sb.ToString(), child));
        }

        return new FlattenedNode(prefix, node.PathLength, node.Value, children);
    }

    private class Node
    {
        public Optional<T> Value { get; set; }

        public required int PathLength { get; set; }

        public SortedDictionary<char, Node> Children { get; } = [];
    }

    private class FlattenedNode
        : IPrefixNode<T>
    {
        private readonly string _prefix;
        private readonly int _pathLength;
        private readonly Optional<T> _value;
        private readonly List<FlattenedNode> _children;

        public FlattenedNode(string prefix, int pathLength, Optional<T> value, List<FlattenedNode> children)
        {
            _prefix = prefix;
            _pathLength = pathLength;
            _value = value;
            _children = children;
        }

        public string Prefix => _prefix;

        public int PathLength => _pathLength;

        public Optional<T> Value => _value;

        public int Count => _children.Count;

        public IEnumerator<IPrefixNode<T>> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

internal interface IPrefixNode<T>
    : IReadOnlyCollection<IPrefixNode<T>>
{
    string Prefix { get; }

    int PathLength { get; }

    Optional<T> Value { get; }
}
