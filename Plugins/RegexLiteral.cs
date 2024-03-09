using JsepSharp.SyntaxTree;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace JsepSharp.Plugins
{
    /// <summary>
    /// Represents an EcmaScript RegExp literal.
    /// </summary>
    /// <remarks>
    /// This is done so the parser doesn't lose flags that C# does not support (e.g. sticky, global).
    /// </remarks>
    public class RegexLiteral : IToStringBuilder
    {
        /// <summary>
        /// The regular expression pattern.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// A string containing regular expression flag characters.
        /// </summary>
        public string Flags { get; set; }

        /// <summary>
        /// A C# representation of the Regular Expression.
        /// </summary>
        /// <remarks>
        /// This is created in part to validate that the expression is valid.
        /// </remarks>
        [JsonIgnore]
        public Regex Object { get; set; }

        /// <summary>
        /// Returns the regular expression as a EcmaScript literal.
        /// </summary>
        /// <returns>A string value.</returns>
        public string ToEcmaScript()
        {
            return $"/{Pattern}/{Flags}";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new NodeStringBuilder();
            ToStringBuilder(sb);
            return sb.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj is RegexLiteral lit) return Equals(lit);

            return false;
        }

        /// <summary>
        /// Determines if another regular expression literal has the same pattern and flags.
        /// </summary>
        /// <param name="node">The literal to compare with this one.</param>
        /// <returns><c>true</c> if the specified instance is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(RegexLiteral lit)
        {
            return lit.Flags == Flags &&
                   lit.Pattern == Pattern;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Pattern, Flags);
        }

        /// <inheritdoc />
        public void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("RegExp");
            sb.EscapedText(Pattern);
            sb.Delim();
            sb.EscapedText(Flags);
            sb.End();
        }

        /// <summary>
        /// Initialize a representation of an EcmaScript RegExp literal.
        /// </summary>
        /// <param name="pattern">The Pattern.</param>
        /// <param name="flags">Optional. Flag characters.</param>
        public RegexLiteral(string pattern, string? flags = null)
        {
            // Only a few flags are supported (i, m) for C#. The .NET class is fairly different in behavior / methods:
            // [https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_expressions#advanced_searching_with_flags]
            // [https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regexoptions?view=net-8.0]
            var rexFlags = RegexOptions.ECMAScript;

            if (flags is not null)
            {
                if (flags.Contains('i')) rexFlags |= RegexOptions.IgnoreCase;
                if (flags.Contains('m')) rexFlags |= RegexOptions.Multiline;
            }

            Flags = flags ?? "";
            Pattern = pattern;
            Object = new Regex(pattern, rexFlags);
        }
    }
}
