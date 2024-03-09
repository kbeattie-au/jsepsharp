using JsepSharp.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepSharp.Plugins.SyntaxTree
{
    public sealed class TemplateElement : SyntaxNode
    {
        const string TYPE_NAME = "TemplateElement";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(TemplateElement), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public TemplateElement() : base() { }

        public TemplateElement(string? raw, string? cooked, bool tail) : base()
        {
            Raw = raw;
            Cooked = cooked;
            Tail = tail;
        }

        public string? Raw { get; set; } // Port: value.raw
        public string? Cooked { get; set; } // Port: value.cooked
        public bool Tail { get; set; }

        public bool ShouldSerializeTail()
        {
            return Tail;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("TElem");
            sb.EscapedText(Raw);
            sb.Delim();
            sb.EscapedText(Cooked);
            sb.Delim();
            sb.Bool(Tail);
            sb.End();
        }

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(TemplateElement node)
        {
            return Raw == node.Raw &&
                   Cooked == node.Cooked &&
                   Tail == node.Tail;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Raw, Cooked, Tail);
        }
    }
}
