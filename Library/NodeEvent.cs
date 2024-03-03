using JsepNet.SyntaxTree;

namespace JsepNet
{
    // Port: This is part of the Hooks.add()/Hooks.remove() replacement.

    /// <summary>
    /// Event argument for parser node operations.
    /// </summary>
    public class NodeEvent
    {
        /// <summary>
        /// The parsed node from the operation.
        /// </summary>
        public SyntaxNode? Node { get; set; }

        public NodeEvent() { }

        public NodeEvent(SyntaxNode? node)
        {
            Node = node;
        }
    }
}
