using JsepSharp.Json;
using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents an entry in the abstract syntax tree.
    /// </summary>
    /// <remarks>
    /// All node types must inherit from this class.
    /// </remarks>
    [JsonConverter(typeof(SyntaxNodeConverter))]
    public abstract class SyntaxNode : IToStringBuilder, IHasOptional
    {
        /// <summary>
        /// The runtime type identifier integer.
        /// </summary>
        [JsonIgnore]
        public abstract int TypeId { get; }

        /// <inheritdoc />
        [JsonProperty(Order = -3)]
        public bool Optional { get; set; }

        string? typ;

        /// <summary>
        /// The name of the node type.
        /// </summary>
        [JsonProperty(Order = -2)]
        public string Type
        {
            get
            {
                if (typ is not null) return typ;

                typ = Jsep.NodeNamesByTypeIds.TryGetValue(TypeId, out string? name) ?
                    name :
                    UnknownNode.TYPE_NAME;

                return typ;
            }
        }

        public bool ShouldSerializeOptional()
        {
            return Optional;
        }

        /// <summary>
        /// Compares two SyntaxNodes for equality.
        /// </summary>
        /// <typeparam name="T">Type of the first node.</typeparam>
        /// <param name="node">The first node.</param>
        /// <param name="other">The second node.</param>
        /// <param name="typedEquals">The typed equality method to use if the other node matches the type of the first node.</param>
        /// <returns>True if nodes are the same type and meet equality rules for the specific type; otherwise, false.</returns>
        public static bool NodeEquals<T>(T? node, object? other, Func<T, bool> typedEquals) where T : SyntaxNode
        {
            if (ReferenceEquals(node, other)) return true;
            if (node is null) return false;
            if (other is SyntaxNode on)
            {
                if (node.TypeId != on.TypeId) return false;

                return typedEquals((T)other);
            }

            return false;
        }

        /// <summary>
        /// Compares if two sequences of nodes contain the same nodes.
        /// </summary>
        /// <param name="nodes">First set of nodes.</param>
        /// <param name="other">Second set of nodes.</param>
        /// <returns>True if nodes are equal; otherwise, false.</returns>
        public static bool SequenceEquals(IEnumerable<SyntaxNode?> nodes, IEnumerable<SyntaxNode?> other)
        {
            if (ReferenceEquals(nodes, other)) return true;

            return nodes is not null && nodes.SequenceEqual(other);
        }

        /// <summary>
        /// Method that checks a node and can optionally replace it with another.
        /// </summary>
        /// <param name="nodeToCheck">Node to examine.</param>
        /// <param name="nodeOut">Replacement node. Set to replacement value if method returns true.</param>
        /// <returns>True if replacement should occur; otherwise, false.</returns>
        public delegate bool NodeReplacer(SyntaxNode? nodeToCheck, out SyntaxNode? nodeOut);

        /// <summary>
        /// Uses supplied method to replaces sub-nodes recursively that meet the expected criteria.
        /// </summary>
        /// <param name="searcher">The searcher method.</param>
        public virtual void ReplaceNodes(NodeReplacer searcher)
        {
            // Default implementation has no sub-nodes, so does nothing.

            // Some plugins (e.g. Arrow, Assignment) need the ability to traverse
            // the full tree and replace nodes as post-processing.
        }

        /// <inheritdoc />
        public virtual void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Unknown");
            sb.EscapedText(base.ToString() ?? "");
            sb.End();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new NodeStringBuilder();
            ToStringBuilder(sb);
            return sb.ToString();
        }
    }
}
