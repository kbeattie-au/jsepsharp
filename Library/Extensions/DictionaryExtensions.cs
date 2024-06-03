using System;

namespace JsepSharp.Extensions
{
#if NET47_OR_GREATER
    /// <summary>
    /// Dictionary extensions for .NET Framework consumers.
    /// </summary>
    /// <remarks>
    /// These are not necessary in .NET Core, which provides these as built-ins.
    /// </remarks>
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Tries to get a value associated with the key from the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="dictionary">Dictionary instance. Required.</param>
        /// <param name="key">Key to use.</param>
        /// <param name="defaultValue">Value to return if an entry for the key is not present.</param>
        /// <returns>Value if one was found for the key in the supplied dictionary; Otherwise, the default value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if dictionary is not supplied.</exception>
        internal static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return defaultValue;
        }

        /// <summary>
        /// Attempts to add the key and value to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="dictionary">Dictionary instance. Required.</param>
        /// <param name="key">Key to use.</param>
        /// <param name="value">Value to store.</param>
        /// <returns>True if the key/value was added successfully to the dictionary; Otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if dictionary is not supplied.</exception>
        internal static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
    }
#endif
}
