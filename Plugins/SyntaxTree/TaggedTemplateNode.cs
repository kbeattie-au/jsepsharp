using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents an tagged template literal.
    /// </summary>
    /// <remarks>
    /// e.g.
    ///   <c>tag`1${a}2${c}3`</c>
    /// </remarks>
    public sealed class TaggedTemplateNode : SyntaxNode
    {
        const string TYPE_NAME = "TaggedTemplateExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(TaggedTemplateNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a tagged template node.
        /// </summary>
        public TaggedTemplateNode() : base() { }

        /// <summary>
        /// Initialize a tagged template node with parameters.
        /// </summary>
        /// <param name="tag">Tag node.</param>
        /// <param name="quasi">Template literal node.</param>
        public TaggedTemplateNode(SyntaxNode? tag, TemplateLiteralNode? quasi) : base()
        {
            Tag = tag;
            Quasi = quasi;
        }

        /// <summary>
        /// This is the tag before the template literal.
        /// </summary>
        public SyntaxNode? Tag { get; set; }

        /// <summary>
        /// This represents the rest of the template literal.
        /// </summary>
        public TemplateLiteralNode? Quasi { get; set; }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (Tag is not null) { yield return Tag; }
            if (Quasi is not null) { yield return Quasi; }
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            // Note: Skips Quasi property by design. Replacement not currently permitted.
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

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same tag and quasi as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(TaggedTemplateNode node)
        {
            return Equals(Tag, node.Tag) &&
                   Equals(Quasi, node.Quasi);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Tag, Quasi);
        }
    }
}
