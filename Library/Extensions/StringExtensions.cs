namespace JsepNet.Extensions
{
    /// <summary>
    /// String extension methods behavior closer to the JavaScript equivalents.
    /// Specifically, methods that avoid throwing range-related exceptions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Gets character at index given or returns a zero terminator if the index is invalid.
        /// </summary>
        /// <param name="str">String instance.</param>
        /// <param name="index">Index in string.</param>
        /// <returns>A character value.</returns>
        public static char CharAt(this string? str, int index) // charAt/charCodeAt
        {
            if (str is null || index >= str.Length || index < 0) return Jsep.NO_MORE;

            return str[index];
        }

        /// <summary>
        /// Returns text between the start and end indices from the string instance.
        /// </summary>
        /// <remarks>
        /// Invalid or backwards indices will result in an empty return value. Similar to JavaScript String.substring() and String.slice(), but does not support negative values.
        /// </remarks>
        /// <param name="str">String instance.</param>
        /// <param name="indexStart">Starting index.</param>
        /// <param name="indexEnd">Ending index.</param>
        /// <returns>The extracted string.</returns>
        public static string FromTo(this string? str, int indexStart, int indexEnd)
        {
            return SubstringSafe(str, indexStart, indexEnd - indexStart);
        }

        /// <summary>
        /// Gets text at the starting position to the end of the string.
        /// </summary>
        /// <remarks>
        /// An invalid starting position results in an empty return value. Similar to JavaScript String.substr(), but does not support negative values.
        /// </remarks>
        /// <param name="str">String instance.</param>
        /// <param name="start">Starting position.</param>
        /// <returns>The extracted string.</returns>
        public static string SubstringSafe(this string? str, int start)
        {
            if (str is null || start < 0 || start >= str.Length) return "";

            return str;
        }

        /// <summary>
        /// Gets text at the starting position, up to the length given.
        /// </summary>
        /// <remarks>
        /// An invalid starting position or negative length result in an empty return value. Similar to JavaScript String.substr(), but does not support negative values.
        /// </remarks>
        /// <param name="str">String instance.</param>
        /// <param name="start">The starting position.</param>
        /// <param name="length">Length to retrieve. If not enough characters remain, the return value may have less characters than this value.</param>
        /// <returns>The extracted string.</returns>
        public static string SubstringSafe(this string? str, int start, int length)
        {
            if (str is null || start < 0 || length < 1) return "";

            var strLen = str.Length;
            if (start >= strLen) return "";
            if (start + length > strLen) return str[start..];

            return str.Substring(start, length);
        }
    }
}
