using JsepSharp.Extensions;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents an array expression.
    /// </summary>
    public sealed class ArrayNode : SyntaxNode
    {
        const string TYPE_NAME = "ArrayExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ArrayNode), TYPE_NAME);

        /// <inheritdoc />
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// The elements of the array.
        /// </summary>
        public List<SyntaxNode?> Elements { get; set; }

        /// <summary>
        /// Initializes an instance with an empty element list. 
        /// </summary>
        public ArrayNode() : base()
        {
            Elements = [];
        }

        /// <summary>
        /// Initializes an instance with an explicit element list.
        /// </summary>
        /// <param name="elements">Entries of the array.</param>
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
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Elements.Compact();
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
