using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins
{
    /// <summary>
    /// Adds support for parsing object literals.
    /// </summary>
    public sealed class ObjectPlugin : Plugin
    {
        /// <inheritdoc />
        public override string Name => "Object";

        public ObjectPlugin(Jsep parser) : base(parser)
        {
            parser.BeforeToken += Parser_BeforeToken;
        }

        // Port: 'gobble-token' hook.
        void Parser_BeforeToken(NodeEvent env)
        {
            var node = GobbleObjectExpression();
            if (node is null) return;
            
            env.Node = node;
        }

        /// <summary>
        /// Look for and parse object literal if it is present.
        /// </summary>
        /// <param name="parser">Required. Parser instance.</param>
        /// <returns>A SyntaxNode instance or null if no object literal found.</returns>
        SyntaxNode? GobbleObjectExpression()
        {
            if (Parser.CharCode == Jsep.OCURLY_CODE)
            {
                Parser.Index++;
                List<SyntaxNode?> properties = [];

                while (Parser.CharCode != Jsep.NO_MORE)
                {
                    Parser.GobbleSpaces();

                    if (Parser.CharCode == Jsep.CCURLY_CODE)
                    {
                        Parser.Index++;
                        return Parser.GobbleTokenProperty(new ObjectNode(properties));
                    }

                    // Note: using GobbleExpression instead of GobbleToken to support object destructuring
                    var key = Parser.GobbleExpression();
                    if (key is null)
                    {
                        break; // missing }
                    }

                    Parser.GobbleSpaces();
                    if (key is IdentifierNode && (Parser.CharCode == Jsep.COMMA_CODE || Parser.CharCode == Jsep.CCURLY_CODE))
                    {
                        // Property value shorthand.
                        properties.Add(new ObjectProperty(false, key, key, true));
                    }
                    else if (Parser.CharCode == Jsep.COLON_CODE)
                    {
                        Parser.Index++;

                        var value = Parser.GobbleExpression() ??
                            throw Parser.Error("Unexpected object property");

                        // Port: replaced `var computed = ...` logic with something a bit different due to C# type behavior differences.
                        if (key is ArrayNode k)
                        {
                            SyntaxNode? keyFirst = k.Elements.FirstOrDefault() ??
                                throw Parser.Error("Key missing");

                            properties.Add(new ObjectProperty(true, keyFirst, value, false));
                        }
                        else
                        {
                            properties.Add(new ObjectProperty(false, key, value, false));
                        }

                        Parser.GobbleSpaces();
                    }
                    else if (key is not null)
                    {
                        // Spread, assignment (object destructuring with defaults), etc.
                        properties.Add(key);
                    }

                    if (Parser.CharCode == Jsep.COMMA_CODE)
                    {
                        Parser.Index++;
                    }
                }

                throw Parser.Error("Missing }");
            }

            return null;
        }
    }
}
