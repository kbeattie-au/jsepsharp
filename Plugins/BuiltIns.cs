namespace JsepSharp.Plugins
{
    /// <summary>
    /// Built-in plugins.
    /// </summary>
    public static class BuiltIns
    {
        // Important: The order of these matters for correct parsing behavior!
        readonly static Type[] allPlugins = [
            typeof(TernaryPlugin),
            typeof(NewPlugin),
            typeof(ObjectPlugin),
            typeof(SpreadPlugin),
            typeof(CommentPlugin),
            typeof(ArrowPlugin),
            typeof(AsyncAwaitPlugin),
            typeof(TemplateLiteralPlugin),
            typeof(AssignmentPlugin),
            typeof(RegexPlugin),
            typeof(NumbersPlugin)
        ];

        /// <summary>
        /// All available plugin types from the built-in plugins library.
        /// </summary>
        public static IEnumerable<Type> All { get => allPlugins; }
    }
}
