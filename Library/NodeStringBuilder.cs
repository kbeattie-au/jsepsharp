using JsepSharp.SyntaxTree;
using System.Text;

namespace JsepSharp
{
    /// <summary>
    /// An output buffer for SyntaxNode to string conversion.
    /// </summary>
    public sealed class NodeStringBuilder
    {
        readonly StringBuilder buffer;
        readonly Stack<int> numEntryStack = new();
        int numEntries = 0;
        int nesting = 0;

        /// <summary>
        /// Controls if output resembles S-expressions. Default is false.
        /// </summary>
        /// <remarks>
        /// Arrays continue to be wrapped in brackets instead of parenthesis.
        /// </remarks>
        public bool UseSExpressions { get; set; } = false;


        /// <summary>
        /// Controls if whitespace indention is included.
        /// </summary>
        public bool UseAutoIndent { get; set; } = false;

        /// <summary>
        /// Initializes instance and internal buffer.
        /// </summary>
        public NodeStringBuilder()
        {
            buffer = new StringBuilder();
        }

        /// <summary>
        /// Invoked when a SyntaxNode is beginning buffer output.
        /// </summary>
        public void Start(string name)
        {
            NestingIncrement();

            if (UseSExpressions)
            {
                
                buffer.Append('(');
                buffer.Append(name);
                buffer.Append(' ');
                AutoIndent();
                return;
            }

            buffer.Append(name);
            buffer.Append('(');
            AutoIndent();
        }

        /// <summary>
        /// Invoked when a SyntaxNode has completed buffer output.
        /// </summary>
        public void End()
        {
            if (numEntries == 0)
            {
                CleanExtraWhitespace();
            }

            buffer.Append(')');

            NestingDecrement();

            if (nesting == 0 && !UseSExpressions)
            {
                buffer.Append(';');
            }
        }

        /// <summary>
        /// Adds a double precision literal to the output buffer.
        /// </summary>
        /// <param name="num">The double value to convert to a string and add.</param>
        public void Number(double num)
        {
            ++numEntries;

            buffer.Append(num);
        }

        /// <summary>
        /// Adds delimiter to output buffer.
        /// </summary>
        public void Delim()
        {
            if (UseSExpressions)
            {
                Next();
                return;
            }

            buffer.Append(',');
            Next();
        }

        /// <summary>
        /// Adds a true delimited boolean literal, but only if the value is true.
        /// </summary>
        /// <param name="val">Value of Optional attribute on the SyntaxNode.</param>
        /// <param name="onlyArgument">If true, the delimiter before the literal will be skipped.></param>
        public void OptionalArgument(bool val, bool onlyArgument = false)
        {
            if (onlyArgument)
            {
                if (val) { Bool(true); }

                return;
            }

            if (val)
            {
                Delim();
                Bool(true);
            }
        }

        /// <summary>
        /// Adds a lowercase boolean literal to the output buffer.
        /// </summary>
        /// <param name="v">Boolean value.</param>
        public void Bool(bool v)
        {
            ++numEntries;

            buffer.Append(v.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Adds a SyntaxNode to the output buffer.
        /// </summary>
        /// <param name="sn">SyntaxNode instance or null.</param>
        public void Node(SyntaxNode? sn)
        {
            if (sn is null)
            {
                Null();
            }
            else
            {
                ++numEntries;

                sn.ToStringBuilder(this);
            }
        }

        /// <summary>
        /// Adds null literal to the output buffer.
        /// </summary>
        public void Null()
        {
            ++numEntries;

            buffer.Append("null");
        }

        /// <summary>
        /// Adds an escaped string wrapped in double quotes to the output buffer.
        /// </summary>
        /// <param name="text">A string value or null.</param>
        public void EscapedText(string? text)
        {
            if (text is null)
            {
                Null();
            }
            else
            {
                ++numEntries;

                buffer.Append('"');
                EncodeEscapeChars(buffer, text);
                buffer.Append('"');
            }
        }

        /// <summary>
        /// Adds a literal, node, escaped string, or null to the output buffer.
        /// </summary>
        /// <typeparam name="T">Type of object being added.</typeparam>
        /// <param name="obj">Object to convert to string representation and add to the buffer.</param>
        public void Any<T>(T? obj)
        {
            if (obj == null)
            {
                Null();
            }
            else if (obj is string s)
            {
                EscapedText(s);
            }
            else if (obj is bool b)
            {
                Bool(b);
            }
            else if (obj is double d)
            {
                Number(d);
            }
            else if (obj is IToStringBuilder tsb)
            {
                ++numEntries;

                tsb.ToStringBuilder(this);
            }
            else
            {
                EscapedText(obj.ToString());
            }
        }

        /// <summary>
        /// Adds a list of nodes to the output buffer separated by delimiters.
        /// </summary>
        /// <typeparam name="T">Type of SyntaxNode.</typeparam>
        /// <param name="nodes">Zero or more syntax nodes.</param>
        public void NodeSequence<T>(IList<T?> nodes) where T : SyntaxNode
        {
            if (nodes.Count == 0) return;

            int i = 0;
            Node(nodes[i++]);

            for (; i < nodes.Count; i++)
            {
                Delim();
                Node(nodes[i]);
            }
        }

        /// <summary>
        /// Adds a list of nodes to the output buffer as a sequence surrounded by brackets.
        /// </summary>
        /// <typeparam name="T">Type of SyntaxNode.</typeparam>
        /// <param name="nodes">Zero or more syntax nodes.</param>
        public void NodeArray<T>(IList<T?> nodes) where T : SyntaxNode
        {
            if (nodes.Count == 0)
            {
                // Empty array still counts as an entry.
                ++numEntries;
            }

            buffer.Append('[');
            NodeSequence(nodes);
            buffer.Append(']');
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return buffer.ToString();
        }

        // Increment nesting level.
        void NestingIncrement()
        {
            if (nesting > 0)
            {
                numEntryStack.Push(numEntries);
            }

            numEntries = 0;
            ++nesting;
        }

        // Decrement nesting level.
        void NestingDecrement()
        {
            if (nesting > 0)
            {
                --nesting;

                if (nesting > 0)
                {
                    numEntries = numEntryStack.Pop();
                }
            }
        }

        // Performs auto-indention if it is enabled.
        bool AutoIndent()
        {
            if (UseAutoIndent)
            {
                buffer.Append(Environment.NewLine);
                buffer.Append(" ".PadRight(nesting * 4));
                return true;
            }

            return false;
        }

        // Adds auto indention or spacing between strings.
        void Next()
        {
            if (!AutoIndent())
            {
                buffer.Append(' ');
            }
        }

        // Removes extra whitespace added when End() is immediately invoked after Start() with no entries.
        void CleanExtraWhitespace()
        {
            var len = buffer.Length;
            var i = len - 1;
            var numToRemove = 0;

            while (i > -1 && char.IsWhiteSpace(buffer[i]))
            {
                ++numToRemove;
                --i;
            }

            if (numToRemove > 0)
            {
                if (i >= 0)
                {
                    buffer.Remove(len - numToRemove, numToRemove);
                }
                else
                {
                    buffer.Clear();
                }
            }
        }

        // Populates buffer with supplied test, converting specific characters to escape sequences.
        static void EncodeEscapeChars(StringBuilder sb, string text)
        {
            foreach (char x in text)
            {
                switch (x)
                {
                    case '\\':
                        sb.Append(@"\\");
                        break;
                    case '\0':
                        sb.Append(@"\0");
                        break;
                    case '\n':
                        sb.Append(@"\n");
                        break;
                    case '\r':
                        sb.Append(@"\r");
                        break;
                    case '\t':
                        sb.Append(@"\t");
                        break;
                    case '\b':
                        sb.Append(@"\b");
                        break;
                    case '\f':
                        sb.Append(@"\f");
                        break;
                    case '\v':
                        sb.Append(@"\v");
                        break;
                    default:
                        sb.Append(x);
                        break;
                }
            }
        }
    }
}
