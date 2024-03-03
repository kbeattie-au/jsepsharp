namespace JsepNet.Extensions
{
    public static class CharSpanExtensions
    {
        /// <summary>
        /// Gets character at index given or returns a zero terminator if the index is invalid.
        /// </summary>
        /// <param name="str">Character span.</param>
        /// <param name="index">Index in character span.</param>
        /// <returns>A character value.</returns>
        public static char CharAt(this ReadOnlySpan<char> str, int index) // charAt/charCodeAt
        {
            if (index >= str.Length || index < 0) return Jsep.NO_MORE;

            return str[index];
        }


        /// <summary>
        /// Returns text between the start and end indices from the character span.
        /// </summary>
        /// <remarks>
        /// Invalid or backwards indices will result in an empty return value. Similar to JavaScript String.substring() and String.slice(), but does not support negative values.
        /// </remarks>
        /// <param name="str">Character span.</param>
        /// <param name="indexStart">Starting index.</param>
        /// <param name="indexEnd">Ending index.</param>
        /// <returns>The extracted span.</returns>
        public static ReadOnlySpan<char> FromTo(this ReadOnlySpan<char> str, int indexStart, int indexEnd)
        {
            return SliceSafe(str, indexStart, indexEnd - indexStart);
        }

        /// <summary>
        /// Gets span of characters at the starting position to the end of the character span.
        /// </summary>
        /// <remarks>
        /// An invalid starting position results in an empty return value. Similar to JavaScript String.substr(), but does not support negative values.
        /// </remarks>
        /// <param name="str">Character span.</param>
        /// <param name="start">Starting position.</param>
        /// <returns>The extracted span.</returns>
        public static ReadOnlySpan<char> SliceSafe(this ReadOnlySpan<char> str, int start)
        {
            if (start < 0 || start >= str.Length) return "".AsSpan();

            return str;
        }

        /// <summary>
        /// Gets span of characters at the starting position, up to the length given.
        /// </summary>
        /// <remarks>
        /// An invalid starting position or negative length result in an empty return value. Similar to JavaScript String.substr(), but does not support negative values.
        /// </remarks>
        /// <param name="str">Character span.</param>
        /// <param name="start">The starting position.</param>
        /// <param name="length">Length to retrieve. If not enough characters remain, the return value may have less characters than this value.</param>
        /// <returns>The extracted span.</returns>
        public static ReadOnlySpan<char> SliceSafe(this ReadOnlySpan<char> str, int start, int length)
        {
            if (start < 0 || length < 1) return "";

            var strLen = str.Length;
            if (start >= strLen) return "";
            if (start + length > strLen) return str[start..];

            return str.Slice(start, length);
        }

    }
}
