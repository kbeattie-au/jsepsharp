using System.Diagnostics.CodeAnalysis;

namespace JsepSharp
{
    /// <summary>
    /// Plugin base class. All JsepSharp plugins must implement this class.
    /// </summary>
    public abstract class Plugin
    {
        protected Jsep Parser { get; set; }

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Plugins that must be loaded for this plugin to work properly.
        /// </summary>
        public virtual IEnumerable<Type> DependentPlugins { get => Array.Empty<Type>(); }

        /// <summary>
        /// Initialize a new plugin instance.
        /// </summary>
        /// <param name="parser">Parser instance.</param>
        [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Want separate XML documentation comments.")]
        public Plugin(Jsep parser)
        {
            Parser = parser;
        }
    }
}
