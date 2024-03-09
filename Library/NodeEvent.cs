using JsepSharp.SyntaxTree;

namespace JsepSharp
{
    // Port: This is part of the Hooks.add()/Hooks.remove() replacement.

    /// <summary>
    /// Event argument for parser node operations.
    /// </summary>
    public class NodeEvent
    {
        /// <summary>
        /// The parsed node from the operation. If set in certain events, this replaces the output of a method.
        /// This is used by plugins to add custom parsing behavior before or after the standard parsing behavior.
        /// </summary>
        public SyntaxNode? Node { get; set; }

        /// <summary>
        /// Initializes without a SyntaxNode.
        /// </summary>
        public NodeEvent() { }

        /// <summary>
        /// Initializes with a SyntaxNode.
        /// </summary>
        /// <param name="node">SyntaxNode instance or null.</param>
        public NodeEvent(SyntaxNode? node)
        {
            Node = node;
        }
    }
}
