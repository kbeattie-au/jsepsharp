using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents an string portions inside a template literal.
    /// </summary>
    public sealed class TemplateElement : SyntaxNode
    {
        const string TYPE_NAME = "TemplateElement";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(TemplateElement), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a template element node.
        /// </summary>
        public TemplateElement() : base() { }

        /// <summary>
        /// Initialize a template element node with parameters.
        /// </summary>
        /// <param name="raw">The raw string.</param>
        /// <param name="cooked">The processed string (escape characters processed).</param>
        /// <param name="tail">Whether this is the last template element.</param>
        public TemplateElement(string? raw, string? cooked, bool tail) : base()
        {
            Raw = raw;
            Cooked = cooked;
            Tail = tail;
        }

        /// <summary>
        /// The unparsed, raw string.
        /// </summary>
        public string? Raw { get; set; } // Port: value.raw

        /// <summary>
        /// The string with escape characters resolved.
        /// </summary>
        public string? Cooked { get; set; } // Port: value.cooked

        /// <summary>
        /// Indiciates that no further elements remain.
        /// </summary>
        public bool Tail { get; set; }

        /// <summary>
        /// Whether tail should be serialized in the output JSON.
        /// </summary>
        /// <returns>True if serialized; Otherwise false.</returns>
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
        public bool Equals(TemplateElement node)
        {
            return Raw == node.Raw &&
                   Cooked == node.Cooked &&
                   Tail == node.Tail;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Raw, Cooked, Tail);
        }
    }
}
