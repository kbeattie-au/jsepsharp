using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents an array expression.
    /// </summary>
    public sealed class ArrayNode : SyntaxNode
    {
        const string TYPE_NAME = "ArrayExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ArrayNode), TYPE_NAME);

        public override int TypeId => NodeTypeId;

        public List<SyntaxNode?> Elements { get; set; }

        public ArrayNode() : base()
        {
            Elements = [];
        }

        public ArrayNode(List<SyntaxNode?> elements) : base()
        {
            Elements = elements;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Ar");
            sb.NodeSequence(Elements);
            sb.End();
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                var node = Elements[i];
                if (searcher(node, out var nodeOut))
                {
                    Elements[i] = nodeOut;
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
        public bool Equals(ArrayNode node)
        {
            return SequenceEquals(Elements, node.Elements);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Elements?.GetHashCode() ?? 0;
        }
    }
}
