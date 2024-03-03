using JsepNet.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepNet.Plugins.SyntaxTree
{
    public sealed class ObjectNode : SyntaxNode
    {
        const string TYPE_NAME = "ObjectExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ObjectNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public ObjectNode() : base()
        {
            Properties = [];
        }

        public ObjectNode(List<SyntaxNode?> properties) : base()
        {
            Properties = properties;
        }

        public List<SyntaxNode?> Properties { get; set; }

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

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(ObjectNode node)
        {
            return SequenceEquals(Properties, node.Properties);
        }

        public override int GetHashCode()
        {
            return Properties?.GetHashCode() ?? 0;
        }
    }
}
