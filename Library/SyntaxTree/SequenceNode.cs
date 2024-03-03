using System.Text.Json.Serialization;

namespace JsepNet.SyntaxTree
{
    public sealed class SequenceNode : SyntaxNode
    {
        const string TYPE_NAME = "SequenceExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(SequenceNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public SequenceNode() : base()
        {
            Expressions = [];
        }

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

        public List<SyntaxNode?> Expressions { get; set; }

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
