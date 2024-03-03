namespace JsepNet.Extensions
{
    /// <summary>
    /// Adds Pop/Push() to lists, since Stack<T> doesn't supported indexed[] access,
    /// which the order of operations algorithm needs. Differs from Stack<T>.Pop() in that
    /// it also doesn't throw when it's out of items (InvalidOperationException).
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Adds item to end of list.
        /// </summary>
        /// <typeparam name="T">Type of entry.</typeparam>
        /// <param name="list">Required. List instance.</param>
        /// <param name="item">Item to add.</param>
        public static void Push<T>(this List<T> list, T item)
        {
            list.Add(item);
        }

        /// <summary>
        /// Gets and removes last item from the list.
        /// </summary>
        /// <typeparam name="T">Type of entry.</typeparam>
        /// <param name="list">Required. List instance.</param>
        /// <returns>Value if list was non-empty, else the default value for the type parameter given.</returns>
        public static T? Pop<T>(this List<T> list)
        {
            var idx = list.Count - 1;
            if (idx < 0) return default;

            var entry = list[idx];
            list.RemoveAt(idx);
            return entry;
        }

        /// <summary>
        /// Gets and removes last item from list, then casts it to a specific type.
        /// </summary>
        /// <typeparam name="T">Type of entries.</typeparam>
        /// <typeparam name="T2">Type to cast popped value to. Must be a subclass of the first type.</typeparam>
        /// <param name="list">Required. List instance.</param>
        /// <returns>Casted value if list was non-empty, else the default value for the type parameter given.</returns>
        public static T2? PopAndCast<T, T2>(this List<T> list) where T2 : T
        {
            var idx = list.Count - 1;
            if (idx < 0) return default;

            var entry = (T2?)list[idx];
            list.RemoveAt(idx);
            return entry;
        }
    }
}
