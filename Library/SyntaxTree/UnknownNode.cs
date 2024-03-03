using System.Text.Json.Serialization;

namespace JsepNet.SyntaxTree
{
    /// <summary>
    /// Used by JSON deserialization when an expression is not supported.
    /// Plugins must be loaded to support custom types.
    /// </summary>
    public sealed class UnknownNode : SyntaxNode
    {
        internal const string TYPE_NAME = "Unknown";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(MemberNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

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
