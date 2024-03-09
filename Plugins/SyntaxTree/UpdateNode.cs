using JsepSharp.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepSharp.Plugins.SyntaxTree
{
    public sealed class UpdateNode : SyntaxNode
    {
        const string TYPE_NAME = "UpdateExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(UpdateNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public UpdateNode() : base() { }

        public UpdateNode(string? @operator, SyntaxNode? argument, bool prefix) : base()
        {
            Operator = @operator;
            Argument = argument;
            Prefix = prefix;
        }

        public string? Operator { get; set; }
        public SyntaxNode? Argument { get; set; }
        public bool Prefix { get; set; }

        public bool ShouldSerializePrefix()
        {
            return Prefix;
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Argument, out var outArg))
            {
                Argument = outArg;
                outArg?.ReplaceNodes(searcher);
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Update");
            sb.EscapedText(Operator);
            sb.Delim();
            sb.Node(Argument);
            sb.Delim();
            sb.Bool(Prefix);
            sb.End();
        }

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(UpdateNode node)
        {
            return Prefix == Prefix &&
                   Operator == Operator &&
                   Equals(Argument, node.Argument);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Prefix, Operator, Argument);
        }
    }
}
