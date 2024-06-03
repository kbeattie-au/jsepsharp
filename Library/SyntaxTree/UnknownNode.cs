using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Used by JSON deserialization when an expression is encountered that is not supported.
    /// This may be resolved by loading a plugin that supports the type encountered.
    /// </summary>
    /// <remarks>
    /// The parser will never return this, since syntax with unknown semantics is 
    /// treated as an unrecoverable error.
    /// </remarks>
    public sealed class UnknownNode : SyntaxNode
    {
        internal const string TYPE_NAME = "Unknown";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(MemberNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// The name of the unknown node type encountered.
        /// </summary>
        public string UnknownType { get; set; } = "";

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Unknown");
            sb.EscapedText(UnknownType);
            sb.End();
        }
    }
}
