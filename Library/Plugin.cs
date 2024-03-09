namespace JsepSharp
{
    // https://blog.rsuter.com/advanced-newtonsoft-json-dynamically-rename-or-ignore-properties-without-changing-the-serialized-class/
    // https://www.newtonsoft.com/json/help/html/ContractResolver.htm

    // Also:
    // https://devblogs.microsoft.com/dotnet/system-text-json-in-dotnet-7/#contract-customization
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/custom-contracts

    /// <summary>
    /// Plugin interface.
    /// </summary>
    public abstract class Plugin(Jsep parser)
    {
        protected Jsep Parser { get; set; } = parser;

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Plugins that must be loaded for this plugin to work properly.
        /// </summary>
        public virtual IEnumerable<Type> DependentPlugins { get => Array.Empty<Type>(); }
    }
}
