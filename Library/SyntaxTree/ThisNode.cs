using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents a this statement.
    /// </summary>
    public sealed class ThisNode : SyntaxNode
    {
        const string TYPE_NAME = "ThisExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ThisNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a this node.
        /// </summary>
        public ThisNode() : base() { }

        /// <summary>
        /// Initialize a this node with parameters.
        /// </summary>
        /// <param name="optional">Whether or not an optional indicator (?) was supplied.</param>
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
#pragma warning restore CA1822
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