﻿using System.Diagnostics.CodeAnalysis;

namespace JsepSharp
{
    /// <summary>
    /// The exception that is thrown when Jsep or a plugin encounters a non-recoverable parsing error.
    /// </summary>
    public sealed class ParsingException : Exception
    {
        /// <summary>
        /// Initializes a new parsing error.
        /// </summary>
        /// <param name="description">The problem encountered.</param>
        /// <param name="index">The character index the problem exists at.</param>
        [SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Documentation of class and constructor is different.")]
        public ParsingException(string description, int index) : base($"{description} at character {index}.")
        {
            Index = index;
            Description = description;
        }

        /// <summary>
        /// The 1-based position of the character where a problem was encountered.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The description of the parsing problem.
        /// </summary>
        public string Description { get; }
    }
}
