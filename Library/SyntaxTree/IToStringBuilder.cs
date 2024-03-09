namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Describes implementations that support a buffered builder 
    /// for instance to string conversion.
    /// </summary>
    public interface IToStringBuilder
    {
        /// <summary>
        /// Writes string representation of instance to supplier builder instance.
        /// </summary>
        /// <param name="sb">Builder instance.</param>
        void ToStringBuilder(NodeStringBuilder sb);
    }
}
