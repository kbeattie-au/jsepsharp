using JsepSharp.Extensions;
using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents an object with zero or more properties.
    /// </summary>
    public sealed class ObjectNode : SyntaxNode
    {
        const string TYPE_NAME = "ObjectExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ObjectNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize an object node.
        /// </summary>
        public ObjectNode() : base()
        {
            Properties = [];
        }

        /// <summary>
        /// Initialize an object node with properties.
        /// </summary>
        /// <param name="properties">Nodes representing properties on the node.</param>
        public ObjectNode(List<SyntaxNode?> properties) : base()
        {
            Properties = properties;
        }

        /// <summary>
        /// List of properties for the object.
        /// </summary>
        public List<SyntaxNode?> Properties { get; set; }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Properties.Compact();
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                var node = Properties[i];
                if (searcher(node, out var nodeOut))
                {
                    Properties[i] = nodeOut;
                    nodeOut?.ReplaceNodes(searcher);
                }
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Obj");
            sb.NodeSequence(Properties);
            sb.End();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same properties as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(ObjectNode node)
        {
            return SequenceEquals(Properties, node.Properties);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Properties?.GetHashCode() ?? 0;
        }
    }
}
