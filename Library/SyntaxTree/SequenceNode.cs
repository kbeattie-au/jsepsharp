using JsepSharp.Extensions;
using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents an sequence expression.<br />
    /// e.g.
    ///   <c>(a b c)</c>
    /// </summary>
    public sealed class SequenceNode : SyntaxNode
    {
        const string TYPE_NAME = "SequenceExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(SequenceNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a sequence node.
        /// </summary>
        public SequenceNode() : base()
        {
            Expressions = [];
        }

        /// <summary>
        /// Initialize a sequence node with parameters.
        /// </summary>
        /// <param name="expressions">Nodes within the sequence.</param>
        public SequenceNode(List<SyntaxNode?> expressions) : base()
        {
            Expressions = expressions;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Seq");
            sb.NodeSequence(Expressions);
            sb.End();
        }

        /// <summary>
        /// Zero or more nodes that make up the sequence.
        /// </summary>
        public List<SyntaxNode?> Expressions { get; set; }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Expressions.Compact();
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            for (int i = 0; i < Expressions.Count; i++)
            {
                var node = Expressions[i];
                if (searcher(node, out var nodeOut))
                {
                    Expressions[i] = nodeOut;
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
        /// Determines if another node of the same type has the same values and entries as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(SequenceNode node)
        {
            return SequenceEquals(Expressions, node.Expressions);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Expressions?.GetHashCode() ?? 0;
        }
    }
}
