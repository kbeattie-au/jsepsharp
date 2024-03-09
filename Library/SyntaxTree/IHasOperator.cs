namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Describes implementations that support an Operator property.
    /// </summary>
    public interface IHasOperator
    {
        /// <summary>
        /// Operator name.
        /// </summary>
        string? Operator { get; set; }
    }
}
