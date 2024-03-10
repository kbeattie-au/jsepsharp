using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents an compound expression.<br />
    /// e.g.
    ///   <c>a, b, c</c>
    /// </summary>
    public sealed class CompoundNode : SyntaxNode
    {
        const string TYPE_NAME = "Compound";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(CompoundNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public CompoundNode() : base()
        {
            Body = [];
        }

        public CompoundNode(List<SyntaxNode?> body) : base()
        {
            Body = body;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Comp");
            sb.NodeSequence(Body);
            sb.End();
        }

        /// <summary>
        /// Zero or more nodes that make up the compound.
        /// </summary>
        public List<SyntaxNode?> Body { get; set; }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            for (int i = 0; i < Body.Count; i++)
            {
                var node = Body[i];
                if (searcher(node, out var nodeOut))
                {
                    Body[i] = nodeOut;
                    nodeOut?.ReplaceNodes(searcher);
                }
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same entries as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(CompoundNode node)
        {
            return SequenceEquals(Body, node.Body);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Body?.GetHashCode() ?? 0;
        }
    }
}
