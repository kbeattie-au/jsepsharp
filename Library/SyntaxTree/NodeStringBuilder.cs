using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace JsepNet.SyntaxTree
{
    public sealed class NodeStringBuilder
    {
        readonly StringBuilder buffer;
        JsonSerializer? serializer = null;
        StringBuilder? serializerBuffer = null;

        public NodeStringBuilder()
        {
            buffer = new StringBuilder();
        }

        public void Append(string text)
        {
            buffer.Append(text);
        }

        public void Append(double num)
        {
            buffer.Append(num);
        }

        public void Start(string name)
        {
            buffer.Append(name);
            buffer.Append('(');
        }

        public void End()
        {
            buffer.Append(')');
        }

        public void Delim()
        {
            buffer.Append(", ");
        }

        public void OptionalArgument(bool val, bool only = false)
        {
            if (only)
            {
                if (val)
                {
                    buffer.Append("true");
                }

                return;
            }

            buffer.Append(", true");
        }

        public override string ToString()
        {
            return buffer.ToString();
        }

        public void Bool(bool v)
        {
            buffer.Append(v.ToString().ToLowerInvariant());
        }

        public void Node(SyntaxNode? sn)
        {
            if (sn is null)
            {
                buffer.Append("null");
            }
            else
            {
                sn.ToStringBuilder(this);
            }
        }

        public void EscapedText(string? text)
        {
            if (text is null)
            {
                buffer.Append("null");
            }
            else
            {
                buffer.Append(ToJson(text));
            }
        }

        public void Any<T>(T? obj)
        {
            if (obj == null)
            {
                buffer.Append("null");
            }
            else if (obj is string)
            {
                buffer.Append(ToJson(obj));
            }
            else if (obj is bool b)
            {
                Bool(b);
            }
            else if (obj is IToStringBuilder tsb)
            {
                tsb.ToStringBuilder(this);
            }
            else
            {
                buffer.Append(obj.ToString() ?? "null");
            }
        }

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

        public void NodeArray<T>(IList<T?> nodes) where T : SyntaxNode
        {
            buffer.Append('[');
            NodeSequence(nodes);
            buffer.Append(']');
        }

        private string ToJson(object? v)
        {
            if (serializer is null)
            {
                serializerBuffer = new StringBuilder(256);
                serializer = JsonSerializer.CreateDefault();
                serializer.Formatting = Formatting.None;
            }
            else
            {
                serializerBuffer!.Clear();
            }

            StringWriter sw = new(serializerBuffer, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new(sw))
            {
                jsonWriter.Formatting = serializer.Formatting;
                serializer.Serialize(jsonWriter, v);
            }

            return sw.ToString();
        }
    }
}
