using System.Text.Json.Serialization;

namespace JsepSharp.SyntaxTree
{
    public sealed class ThisNode : SyntaxNode
    {
        const string TYPE_NAME = "ThisExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ThisNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public ThisNode() : base() { }

        public ThisNode(bool optional) : this()
        {
            Optional = optional;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Self");
            sb.OptionalArgument(Optional, true);
            sb.End();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

#pragma warning disable CA1822 // Mark members as static
        /// <summary>
        /// Determines if another node of the same type has the same values as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns>Always <c>true</c>.</returns>
        public bool Equals(ThisNode node)
#pragma warning restore CA1822 // Mark members as static
        {
            return true;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}