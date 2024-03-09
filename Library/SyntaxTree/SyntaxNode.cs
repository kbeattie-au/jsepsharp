using JsepSharp.Json;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace JsepSharp.SyntaxTree
{
    [JsonConverter(typeof(SyntaxNodeConverter))]
    public abstract class SyntaxNode : IToStringBuilder
    {
        [JsonIgnore]
        public abstract int TypeId { get; }

        public bool Optional { get; set; }

        private string? typ;
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

        public static bool SequenceEquals(IEnumerable<SyntaxNode?> nodes, IEnumerable<SyntaxNode?> other)
        {
            if (ReferenceEquals(nodes, other)) return true;

            return nodes is not null && nodes.SequenceEqual(other);
        }

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
            sb.Append(base.ToString() ?? "");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new NodeStringBuilder();
            ToStringBuilder(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Designed for plugins to store extra data for nodes.
        /// </summary>
        public readonly static ConditionalWeakTable<SyntaxNode, Dictionary<string, object?>> Metadata = [];
    }
}
