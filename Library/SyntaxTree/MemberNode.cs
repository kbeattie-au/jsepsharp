using System.Text.Json.Serialization;

namespace JsepNet.SyntaxTree
{
    public sealed class MemberNode : SyntaxNode
    {
        const string TYPE_NAME = "MemberExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(MemberNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public MemberNode() : base() { }

        public MemberNode(bool computed, SyntaxNode? @object, SyntaxNode? property) : base()
        {
            Computed = computed;
            Object = @object;
            Property = property;
        }

        public MemberNode(bool computed, SyntaxNode? @object, SyntaxNode? property, bool optional) :
            this(computed, @object, property)
        {
            Optional = optional;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Member");
            sb.Bool(Computed);
            sb.Delim();
            sb.Node(Object);
            sb.Delim();
            sb.Node(Property);
            sb.OptionalArgument(Optional);
            sb.End();
        }

        public bool Computed { get; set; }
        public SyntaxNode? Object { get; set; }
        public SyntaxNode? Property { get; set; }

        public bool ShouldSerializeComputed()
        {
            return Computed;
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Object, out var outObj))
            {
                Object = outObj;
                outObj?.ReplaceNodes(searcher);
            }

            if (searcher(Property, out var outProp))
            {
                Property = outProp;
                outProp?.ReplaceNodes(searcher);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same values as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(MemberNode node)
        {
            return Computed == node.Computed &&
                   Equals(Object, node.Object) &&
                   Equals(Property, node.Property);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Computed, Object, Property);
        }
    }
}
