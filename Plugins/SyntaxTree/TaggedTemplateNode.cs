using JsepNet.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepNet.Plugins.SyntaxTree
{
    public sealed class TaggedTemplateNode : SyntaxNode
    {
        const string TYPE_NAME = "TaggedTemplateExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(TaggedTemplateNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public TaggedTemplateNode() : base() { }

        public TaggedTemplateNode(SyntaxNode? tag, TemplateLiteralNode? quasi) : base()
        {
            Tag = tag;
            Quasi = quasi;
        }

        public SyntaxNode? Tag { get; set; }
        public TemplateLiteralNode? Quasi { get; set; }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            // TODO: Skipping quasi. Is that OK when this plugin is implemented?
            if (searcher(Tag, out var outTag))
            {
                Tag = outTag;
                outTag?.ReplaceNodes(searcher);
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("TagLit");
            sb.Node(Tag);
            sb.Delim();
            sb.Node(Quasi);
            sb.End();
        }

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(TaggedTemplateNode node)
        {
            return Equals(Tag, node.Tag) &&
                   Equals(Quasi, node.Quasi);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag, Quasi);
        }
    }
}
