namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Describes implementations that support an optional chaining indicator.
    /// </summary>
    public interface IHasOptional
    {
        /// <summary>
        /// Whether or not this node has an optional chaining indicator.
        /// </summary>
        public bool Optional { get; set; }
    }
}
