using JsepNet.SyntaxTree;

namespace JsepNet.Extensions
{
    /// <summary>
    /// Extension methods for SyntaxNodes.
    /// </summary>
    public static class SyntaxNodeExtensions
    {
        /// <summary>
        /// Sets metadata for supplied node and key.
        /// </summary>
        /// <param name="node">Required. Node instance.</param>
        /// <param name="key">Key of the value to set.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>True if value set successfully.</returns>
        public static bool TrySetValue(this SyntaxNode node, string key, object value)
        {
            var metadata = SyntaxNode.Metadata;

            if (metadata.TryGetValue(node, out var map))
            {
                return map.TryAdd(key, value);
            }
            else
            {
                map = [];
                if (!metadata.TryAdd(node, map)) return false;

                return map.TryAdd(key, value);
            }
        }

        /// <summary>
        /// Gets metadata for the supplied node and key.
        /// </summary>
        /// <param name="node">Required. Node instance.</param>
        /// <param name="key">Key of value to retrieve.</param>
        /// <param name="value">Output: Value retrieved from storage.</param>
        /// <returns>True if key exists and value retrieved successfully.</returns>
        public static bool TryGetValue(this SyntaxNode node, string key, out object? value)
        {
            if (SyntaxNode.Metadata.TryGetValue(node, out var map))
            {
                if (map.TryGetValue(key, out var val))
                {
                    value = val;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
