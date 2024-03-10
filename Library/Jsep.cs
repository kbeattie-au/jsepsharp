using JsepSharp.Extensions;
using JsepSharp.SyntaxTree;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace JsepSharp
{
    /// <summary>
    /// A .NET Implementation of the JavaScript Expression Parser (Jsep) project.
    /// </summary>
    public sealed class Jsep
    {
        /// <summary>
        /// Creates new instance of the Javascript Expression parser.
        /// </summary>
        /// <param name="expr">Expression to parse.</param>
        public Jsep(string expr)
        {
            Expression = expr;
            Plugins = new(plugins);
        }

        /// <summary>
        /// Version of the library.
        /// </summary>
        public static string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        // Port: static toString()
        /// <summary>
        /// Name of the library.
        /// </summary>
        public static string Name { get; } = $"JavaScript Expression Parser (JSEP.NET) v{Version}";

        // Kept these since it makes it easier to map to the original JavaScript source.
        // Used internally during Parse(), and exposed for plugins to easily use.
        public const char NO_MORE = '\0';
        public const char TAB_CODE = '\t';
        public const char LF_CODE = '\n';
        public const char CR_CODE = '\r';
        public const char SPACE_CODE = ' ';
        public const char PERIOD_CODE = '.';
        public const char COMMA_CODE = ',';
        public const char SQUOTE_CODE = '\'';
        public const char DQUOTE_CODE = '"';
        public const char OPAREN_CODE = '(';
        public const char CPAREN_CODE = ')';
        public const char OBRACK_CODE = '[';
        public const char CBRACK_CODE = ']';
        public const char QUMARK_CODE = '?';
        public const char SEMCOL_CODE = ';';

        // Not used in the core implementation, but by plugins. Kept for same reason.
        public const char OCURLY_CODE = '{';
        public const char CCURLY_CODE = '}';
        public const char ASTSK_CODE = '*';
        public const char AT_CODE = '@';
        public const char BTICK_CODE = '`';
        public const char BSLASH_CODE = '\\';
        public const char COLON_CODE = ':';
        public const char DOLLAR_CODE = '$';
        public const char FSLASH_CODE = '/';
        public const char NUM_0_CODE = '0';
        public const char UNDERSCORE_CODE = '_';

        // Unique to C# implementation. Node identifiers/names lookups.
        static int freeId = 1;
        static readonly Dictionary<Type, int> nodeTypesByTypeId = [];
        /// <summary>A readonly dictionary of node types and type identifiers.</summary>
        public static ReadOnlyDictionary<Type, int> NodeTypesByTypeId { get; } = new(nodeTypesByTypeId);

        static readonly Dictionary<int, string> nodeNamesByTypeIds = [];
        /// <summary>A readonly dictionary of type identifiers and names.</summary>
        public static ReadOnlyDictionary<int, string> NodeNamesByTypeIds { get; } = new(nodeNamesByTypeIds);

        static readonly Dictionary<string, Type> nodeTypesByStrings = [];
        /// <summary>A readonly dictionary of names and node types.</summary>
        public static ReadOnlyDictionary<string, Type> NodeTypesByStrings { get; } = new(nodeTypesByStrings);

        // Tracks if class has performed static initialization.
        static bool classInitialized = false;

        /// <summary>
        /// Initializes static members. Necessary for deserialization before parsing to function properly.
        /// </summary>
        public static void Initialize()
        {
            if (classInitialized) return;

            // Force static initialization of each class, which registers nodes with this class.
            int[] _ = [
                ArrayNode.NodeTypeId,
                BinaryNode.NodeTypeId,
                CallNode.NodeTypeId,
                CompoundNode.NodeTypeId,
                IdentifierNode.NodeTypeId,
                LiteralNode.NodeTypeId,
                MemberNode.NodeTypeId,
                SequenceNode.NodeTypeId,
                ThisNode.NodeTypeId,
                UnaryNode.NodeTypeId,
                UnknownNode.NodeTypeId
            ];

            classInitialized = true;
        }

        // Port: static unary_ops
        static readonly Dictionary<string, double> unaryOpsDefault = new()
        {
            { "-", 1 },
            { "!", 1 },
            { "~", 1 },
            { "+", 1 }
        };
        static Dictionary<string, double> unaryOps = new(unaryOpsDefault);
        /// <summary>A readonly dictionary of unary operators. The value portion is currently ignored, since these do not have precedence.</summary>
        /// <remarks>Kept unused value portion to retain similarity with the JavaScript version.</remarks>
        public static ReadOnlyDictionary<string, double> UnaryOps { get; } = new(unaryOps);

        // Port: static binary_ops
        // See [https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Operator_Precedence#table]
        static readonly Dictionary<string, double> binaryOpsDefault = new()
        {
            { "||", 1 }, { "&&", 2 }, { "|", 3 }, { "^", 4 }, { "&", 5 },
            { "==", 6 }, { "!=", 6 }, { "===", 6 }, { "!==", 6 },
            { "<", 7 }, { ">", 7 }, { "<=", 7 }, { ">=", 7 },
            { "<<", 8 }, { ">>", 8 }, { ">>>", 8 },
            { "+", 9 }, { "-", 9 },
            { "*", 10 }, { "/", 10 }, { "%", 10 },
            // { "**", 11 } <- Right to left, not left to right. Not in JavaScript version, though the Assignment plugin has `**=`.
        };
        static Dictionary<string, double> binaryOps = new(binaryOpsDefault);
        /// <summary>
        /// A readonly dictionary of binary operators. The keys are the operators and the values are the operator precedence.
        /// </summary>
        public static ReadOnlyDictionary<string, double> BinaryOps { get; } = new(binaryOps);

        // Port: plugins.js
        readonly Dictionary<Type, Plugin> plugins = [];
        /// <summary>
        /// A readonly dictionary of registered plugins for this instance.
        /// </summary>
        public ReadOnlyDictionary<Type, Plugin> Plugins;

        // Port: static right_associative
        static readonly HashSet<string> rightAssociativeDefault = [];
        static HashSet<string> rightAssociative = new(rightAssociativeDefault);
        /// <summary>
        /// A readonly set of right-associative operators.
        /// </summary>
#if NET8_0_OR_GREATER
        public static IReadOnlySet<string> RightAssociative { get; } = rightAssociative;
#else
        public static ISet<string> RightAssociative { get; } = rightAssociative;
#endif

        // Port: static additional_identifier_chars
        static readonly HashSet<char> additionalIdentifiersDefault = ['$', '_'];
        static HashSet<char> additionalIdentifiers = new(additionalIdentifiersDefault);
        /// <summary>
        /// A readonly set of additional identifier characters.
        /// </summary>
#if NET8_0_OR_GREATER
        public static IReadOnlySet<char> AdditionalIdentifiers { get; } = additionalIdentifiers;
#else
        public static ISet<char> AdditionalIdentifiers { get; } = additionalIdentifiers;
#endif

        // Port: static literals
        static readonly Dictionary<string, object?> literalsDefault = new()
        {
            { "true", true },
            { "false", false },
            { "null", null }
        };
        static Dictionary<string, object?> literals = new(literalsDefault);
        /// <summary>
        /// A readonly dictionary of literals. The keys are the keywords and the values are the translation.
        /// </summary>
        public static ReadOnlyDictionary<string, object?> Literals { get; } = new(literals);

        // Port: this_str
        /// <summary>
        /// The string used to represent the `this` keyword. May be customized.
        /// </summary>
        public string ThisString = "this";

        static int maxUnopLen = GetMaxKeyLen(unaryOps); // Port: static max_unop_len

        static int maxBinopLen = GetMaxKeyLen(binaryOps); // Port: static max_binop_len

        #region Config

        /// <summary>
        /// Controls if missing unary operator arguments throw a parser error.
        /// </summary>
        public static bool AllowMissingUnaryArgument { get; set; } = false;

        /// <summary>
        /// Registers node type as a well-known type and assigns it an identifier.
        /// </summary>
        /// <remarks>
        /// These identifiers are used by the TypeId property on SyntaxNodes.
        /// </remarks>
        /// <param name="nodeType">A SyntaxNode type.</param>
        /// <param name="name">Name to use for type.</param>
        /// <param name="nodeId">Output: Integer type identifier.</param>
        public static bool TryRegisterNodeType(Type nodeType, string name, out int nodeId)
        {
            if (nodeTypesByTypeId.TryAdd(nodeType, freeId))
            {
                nodeId = freeId;
                ++freeId;
                nodeNamesByTypeIds[nodeId] = name;
                nodeTypesByStrings[name] = nodeType;
                return true;
            }

            nodeId = 0;
            return false;
        }

        /// <summary>
        /// Registers or gets type identifier for supplied type.
        /// </summary>
        /// <param name="type">SyntaxNode-derived type class.</param>
        /// <param name="name">Name of node.</param>
        /// <returns>The node type identifier.</returns>
        /// <exception cref="InvalidOperationException">Lookup and registration failed.</exception>
        public static int GetOrRegisterTypeIdFor(Type type, string name)
        {
            if (nodeTypesByTypeId.TryGetValue(type, out var existId))
            {
                return existId;
            }

            if (TryRegisterNodeType(type, name, out var newId))
            {
                return newId;
            }

            throw new Exception("Cannot lookup or register type identifier.");
        }

        /// <summary>
        /// Registers a plugin that changes the behavior of the parser.
        /// </summary>
        /// <param name="pluginType">Type of plugin register and create, if it has not already been registered.</param>
        /// <returns>True if registration succeeded. False typically indicates the plugin is already present.</returns>
        public bool RegisterPlugin(
            Type pluginType) // Port: plugins.register
        {
            if (plugins.ContainsKey(pluginType)) return false;

            if (!typeof(Plugin).IsAssignableFrom(pluginType))
            {
                throw new ArgumentException($"{pluginType} does not implement abstract class Jsep.Plugin!", nameof(pluginType));
            }

            var p = CreatePlugin(pluginType);

            foreach (var dpt in p.DependentPlugins)
            {
                RegisterPlugin(dpt);
            }

            return plugins.TryAdd(pluginType, p);
        }

        // Creates plugin instance using supplied type.
        Plugin CreatePlugin(Type pluginType)
        {
            return Activator.CreateInstance(pluginType, this) as Plugin ??
                throw new ArgumentException($"Activator.CreateInstance() for {pluginType} returned null!", nameof(pluginType));
        }

        /// <summary>
        /// Adds unary operator to parser.
        /// </summary>
        /// <param name="operator">The name of the unary operator to add.</param>
        /// <returns>True if operator added.</returns>
        public static bool AddUnaryOp(string @operator)
        {
            maxUnopLen = Math.Max(@operator.Length, maxUnopLen);
            return unaryOps.TryAdd(@operator, 1);
        }

        /// <summary>
        /// Adds binary operator to parser.
        /// </summary>
        /// <param name="operator">The name of the binary operator to add.</param>
        /// <param name="precedence">The precedence of the binary op. Higher number = higher precedence.</param>
        /// <param name="isRightAssociative">Whether operator is right-associative.</param>
        /// <returns>True if a new operator was added; False if existing operator was updated.</returns>
        public static bool AddBinaryOp(string @operator, double precedence, bool isRightAssociative)
        {
            maxBinopLen = Math.Max(@operator.Length, maxBinopLen);

            if (isRightAssociative)
            { 
                rightAssociative.Add(@operator);
            }
            else
            {
                rightAssociative.Remove(@operator);
            }

            if (binaryOps.TryAdd(@operator, precedence)) return true;

            binaryOps[@operator] = precedence;
            return false;
        }

        /// <summary>
        /// Adds identifiers to treat as valid parts of identifiers.
        /// </summary>
        /// <param name="ch">The additional characters to treat as a valid part of an identifier.</param>
        /// <returns>True if character added.</returns>
        public static bool AddIdentifier(char ch) // Port: addIdentifierChar
        {
            return additionalIdentifiers.Add(ch);
        }

        /// <summary>
        /// Adds string that is automatically converted to a literal value at parse time.
        /// </summary>
        /// <remarks>
        /// The built-ins are true, false, and null.
        /// </remarks>
        /// <param name="literalName">Literal name.</param>
        /// <param name="literalValue">Replacement value.</param>
        /// <returns>True if literal added; False if existing literal updated.</returns>
        public static bool AddLiteral(string literalName, object? literalValue)
        {
            if (literals.TryAdd(literalName, literalValue)) return true;

            literals[literalName] = literalValue;
            return false;
        }

        /// <summary>
        /// Removes a string from automatic literal handling.
        /// </summary>
        /// <param name="literalName">Literal name.</param>
        /// <returns>True if literal removed.</returns>
        public static bool RemoveUnaryOp(string literalName)
        {
            var ret = unaryOps.Remove(literalName);

            if (literalName.Length == maxUnopLen)
            {
                maxUnopLen = GetMaxKeyLen(unaryOps);
            }

            return ret;
        }

        /// <summary>
        /// Remove all unary operators.
        /// </summary>
        /// <remarks>
        /// Warning: This includes built-in operators.
        /// </remarks>
        public static void RemoveAllUnaryOps()
        {
            unaryOps.Clear();
            maxUnopLen = 0;
        }

        /// <summary>
        /// Resets unary operators to defaults.
        /// </summary>
        public static void ResetUnaryOps()
        {
            unaryOps = new(unaryOpsDefault);
            maxUnopLen = GetMaxKeyLen(unaryOps);
        }

        /// <summary>
        /// Remove characters from being treated as valid from identifiers.
        /// </summary>
        /// <remarks>
        /// You cannot remove core built-in characters.
        /// </remarks>
        /// <param name="ch">The character to stop treating as a valid part of an identifier.</param>
        /// <returns>True if character removed.</returns>
        public static bool RemoveIdentifier(char ch) // Port: removeIdentifier
        {
            return additionalIdentifiers.Remove(ch);
        }

        /// <summary>
        /// Resets additional identifiers operators to defaults.
        /// </summary>
        public static void ResetIdentifiers()
        {
            additionalIdentifiers = new(additionalIdentifiersDefault);
        }

        /// <summary>
        /// Remove binary operator from parser.
        /// </summary>
        /// <param name="operator">Operator name.</param>
        /// <returns>True if operator removed.</returns>
        public static bool RemoveBinaryOp(string @operator)
        {
            var ret = binaryOps.Remove(@operator);

            if (@operator.Length == maxUnopLen)
            {
                maxBinopLen = GetMaxKeyLen(binaryOps);
            }
            rightAssociative.Remove(@operator);

            return ret;
        }

        /// <summary>
        /// Removes all operators.
        /// </summary>
        /// <remarks>
        /// Warning: This includes built-in operators.
        /// </remarks>
        public static void RemoveAllBinaryOps()
        {
            binaryOps.Clear();
            rightAssociative.Clear(); // JavaScript version doesn't do this.
            maxBinopLen = 0;
        }

        /// <summary>
        /// Resets binary operators to defaults.
        /// </summary>
        public static void ResetBinaryOps()
        {
            binaryOps = new(binaryOpsDefault);
            rightAssociative = new(rightAssociativeDefault);
            maxBinopLen = GetMaxKeyLen(binaryOps);
        }

        /// <summary>
        /// Removes string from being automatically converted to a literal value at parse time.
        /// </summary>
        /// <param name="literalName">Literal name.</param>
        /// <returns>True if literal removed.</returns>
        public static bool RemoveLiteral(string literalName)
        {
            return literals.Remove(literalName);
        }

        /// <summary>
        /// Removes all automatic literal conversions.
        /// </summary>
        /// <remarks>
        /// Warning: This includes built-in literals.
        /// </remarks>
        public static void RemoveAllLiterals()
        {
            literals.Clear();
        }

        /// <summary>
        /// Resets binary operators to defaults.
        /// </summary>
        public static void ResetLiterals()
        {
            literals = new(literalsDefault);
        }

        #endregion

        public string Expression { get; private set; } // Port: expr
        public int Index = 0;

        // Port: get char(), get code()
        /// <summary>
        /// Gets current character at the parser index.
        /// </summary>
        public char CharCode
        {
            get
            {
                return Expression.CharAt(Index);
            }
        }

        /// <summary>
        /// Parse numbers like the original JavaScript implementation,
        /// which returns NaN on failures.
        /// </summary>
        /// <param name="parseText">String to parse.</param>
        /// <returns>A valid Double value or <c>double.NaN</c>.</returns>
        public static double ParseFloat(string? parseText)
        {
            if (parseText is null) return double.NaN;

            if (double.TryParse(parseText, out var value)) return value;

            return double.NaN;
        }

        /// <summary>
        /// Returns expression parsed to AST (abstract syntax tree).
        /// </summary>
        /// <remarks>
        /// Creates instance and invokes the parse() method.
        /// </remarks>
        /// <param name="expression">Expression to parse.</param>
        /// <exception cref="ParsingException">Thrown if parsing errors occurred.</exception>
        /// <returns>A top-level node from the parsed AST.</returns>
        public static SyntaxNode Parse(string expression)
        {
            return new Jsep(expression).Parse();
        }

        /// <summary>
        /// Returns expression parsed to AST (abstract syntax tree) using supplied plugins.
        /// </summary>
        /// <remarks>
        /// Creates instance and invokes the parse() method.
        /// </remarks>
        /// <param name="expression">Expression to parse.</param>
        /// <param name="pluginTypes">An enumerable of plugin types to use when parsing.</param>
        /// <exception cref="ParsingException">Thrown if parsing errors occurred.</exception>
        /// <returns>A top-level node from the parsed AST.</returns>
        public static SyntaxNode Parse(string expression, IEnumerable<Type> pluginLoaders)
        {
            Jsep parser = new(expression);

            foreach (var pt in pluginLoaders)
            {
                parser.RegisterPlugin(pt);
            }

            return parser.Parse();
        }

        /**
         * Gets maximum key length for the supplied dictionary.
         */
        static int GetMaxKeyLen<T>(Dictionary<string, T> map)
        {
            return map.Keys.Max(k => k.Length);
        }

        // Port: Skipped isDecimalDigit(), .NET already has this via char.IsAsciiDigit().

        /// <summary>
        /// Gets prededence of a binary operator.
        /// </summary>
        /// <param name="operator">Operator name (e.g. '==', '+')</param>
        /// <returns>Precedence if operator exists, else 0.</returns>
        public static double BinaryPrecedence(string @operator)
        {
            return binaryOps.GetValueOrDefault(@operator, 0);
        }

        static bool IsCharBetween(char ch, char minInclusive, char maxInclusive)
        {
            return (uint)(ch - minInclusive) <= (uint)(maxInclusive - minInclusive);
        }

        /// <summary>
        /// Checks if character is an ASCII digit.
        /// </summary>
        /// <remarks>
        /// Use <c>char.IsAsciiDigit()</c> for newer targets.
        /// </remarks>
        /// <param name="ch">Character to check.</param>
        /// <returns>True if criteria met; otherwise, false.</returns>
        public static bool IsCharAsciiDigit(char ch)
        {
            
            return IsCharBetween(ch, '0', '9');
        }

        /// <summary>
        /// Checks if character is an ASCII letter.
        /// </summary>
        /// <remarks>
        /// Use <c>char.IsAsciiLetter()</c> for newer targets.
        /// </remarks>
        /// <param name="ch">Character to check.</param>
        /// <returns>True if criteria met; otherwise, false.</returns>
        public static bool IsCharAsciiLetter(char ch)
        {
            return ((uint)((ch | 0x20) - 'a') <= 'z' - 'a');
        }

        /// <summary>
        /// Checks if character is an ASCII letter or digit.
        /// </summary>
        /// <remarks>
        /// Use <c>char.IsAsciiLetterOrDigit()</c> for newer targets.
        /// </remarks>
        /// <param name="ch">Character to check.</param>
        /// <returns>True if criteria met; otherwise, false.</returns>
        public static bool IsCharAsciiLetterOrDigit(char ch)
        {
            return IsCharAsciiLetter(ch) | IsCharAsciiDigit(ch);
        }

        /// <summary>
        /// Gets whether or not character is the start of an identifier. 
        /// </summary>
        /// <param name="ch">Character to check.</param>
        /// <returns>True if character can be used to start an identififer.</returns>
        public static bool IsIdentifierStart(char ch)
        {
#if NET8_0_OR_GREATER
            if (char.IsAsciiLetter(ch)) return true;
#else
            if (IsCharAsciiLetter(ch)) return true;
#endif

            string chs = ch.ToString();
            if (ch > 128 && !binaryOps.ContainsKey(chs)) return true;

            return additionalIdentifiers.Contains(ch);
        }

        /// <summary>
        /// Gets whether character is a valid part of an identifier.
        /// </summary>
        /// <param name="ch">Character to check.</param>
        /// <returns>True if a valid part of an identifier.</returns>
        public static bool IsIdentifierPart(char ch)
        {
#if NET8_0_OR_GREATER
            return char.IsAsciiDigit(ch) || IsIdentifierStart(ch);
#else
            return IsCharAsciiDigit(ch) || IsIdentifierStart(ch);
#endif
        }

        /// <summary>
        /// Replaces common escape codes.
        /// </summary>
        /// <param name="ch">Character appearing after a backslash ('\') in a string/template literal.</param>
        /// <returns>The replacement character on replacement, or the original character.</returns>
        public static char ReplaceEscapeChar(char ch)
        {
            return ch switch
            {
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'b' => '\b',
                'f' => '\f',
                'v' => '\x0B',
                _ => ch,
            };
        }

        /// <summary>
        /// Create an exception that captures parsing position and a description of the problem.
        /// </summary>
        /// <param name="message">Description of problem.</param>
        /// <returns>A new exception instance.</returns>
        public ParsingException Error(string message)
        {
            // Port: throwError()
            return new ParsingException(message, Index);
        }

        // Skipped: runHook and searchHook replaced with C# events (multicast delegates) instead.

        /// <summary>
        /// Fired after whitespace is removed.
        /// </summary>
        public event Action? AfterSpaces; // Port: 'gobble-spaces' hook.
        void OnAfterSpaces()
        {
            if (AfterSpaces is not null)
            {
                AfterSpaces();
            }
        }

        /// <summary>
        /// Used to ignores whitespace characters.
        /// </summary>
        public void GobbleSpaces()
        {
            var ch = CharCode;
            while (ch == SPACE_CODE || ch == TAB_CODE || ch == LF_CODE || ch == CR_CODE)
            {
                ch = Expression.CharAt(++Index);
            }

            OnAfterSpaces();
        }

        /// <summary>
        /// Fired before for parsing occurs.
        /// </summary>
        public event Action? BeforeParsing; // Port: 'before-all' hook.
        void OnBeforeParsing()
        {
            if (BeforeParsing is not null)
            {
                BeforeParsing();
            }
        }

        /// <summary>
        /// Fired after parsing finishes.
        /// Each subscriber is supplied an argument with a Node property that can be inspected or replaced.
        /// Replacing the Node property will replace the return value from Parse().
        /// </summary>
        public event Action<NodeEvent>? AfterParsing; // Port: 'after-all' hook.
        SyntaxNode OnAfterParsing(SyntaxNode node)
        {
            if (AfterParsing is not null)
            {
                var eventArgs = new NodeEvent(node);
                AfterParsing(eventArgs);
                node = eventArgs.Node ?? node;
            }

            return node;
        }

        /// <summary>
        /// Returns expression parsed to AST (abstract syntax tree).
        /// </summary>
        /// <exception cref="ParsingException">Thrown if parsing errors occurred.</exception>
        /// <returns>A top-level node from the parsed AST.</returns>
        public SyntaxNode Parse()
        {
            OnBeforeParsing();

            var nodes = GobbleExpressions();

            // If there's only one expression just try returning the expression
            var node = nodes.Count == 1 ? nodes[0] : new CompoundNode(nodes!);

            return OnAfterParsing(node);
        }

        /// <summary>
        /// Top-level parser, but can be reused within (e.g. for parenthesis/groups).
        /// </summary>
        /// <param name="untilICode">Character to stop parsing expressions at.</param>
        /// <returns>A list of zero or more parsed nodes.</returns>
        public List<SyntaxNode> GobbleExpressions(char? untilICode = null)
        {
            List<SyntaxNode> nodes = [];
            char ch_i;
            SyntaxNode? node;

            while (Index < Expression.Length)
            {
                ch_i = CharCode;

                // Expressions can be separated by semicolons, commas,
                // or just inferred without any separators.
                if (ch_i == SEMCOL_CODE || ch_i == COMMA_CODE)
                {
                    Index++; // ignore separators at this level.
                }
                else
                {
                    // Try to gobble each expression individually.
                    node = GobbleExpression();

                    if (node is not null)
                    {
                        nodes.Add(node);
                    }
                    // If we weren't able to find a binary expression and are out of room, then
                    // the expression passed in probably has too much.
                    else if (Index < Expression.Length)
                    {
                        if (ch_i == untilICode)
                        {
                            break;
                        }

                        throw Error($"Unexpected '{CharCode}'");
                    }
                }
            }

            return nodes;
        }

        /// <summary>
        /// Fired before expression is parsed. 
        /// If the Node property on the event argument is set, standard expression parsing is skipped.
        /// </summary>
        public event Action<NodeEvent>? BeforeExpression; // Port: 'gobble-expression' hook.
        SyntaxNode? OnBeforeExpression()
        {
            if (BeforeExpression is null) return null;

            var eventArgs = new NodeEvent();
            BeforeExpression(eventArgs);
            return eventArgs.Node;
        }

        /// <summary>
        /// Fired after expression is parsed. 
        /// Each event subscriber is supplied a node that can be inspected or replaced.
        /// </summary>
        public event Action<NodeEvent>? AfterExpression; // Port: 'after-expression' hook.
        SyntaxNode? OnAfterExpression(SyntaxNode? node)
        {
            if (AfterExpression is not null)
            {
                var eventArgs = new NodeEvent(node);
                AfterExpression(eventArgs);
                node = eventArgs.Node ?? node;
            }

            return node;
        }

        /// <summary>
        /// Main parsing function used by top-level parser. Parses a single expression.
        /// </summary>
        /// <returns>A SyntaxNode or null.</returns>
        public SyntaxNode? GobbleExpression()
        {
            var node = OnBeforeExpression() ?? GobbleBinaryExpression();
            GobbleSpaces();

            return OnAfterExpression(node);
        }

        /// <summary>
        /// Search for the operation portion of the string (e.g. `+`, `===`)
        /// Start by taking the longest possible binary operations (3 characters: `===`, `!==`, `>>>`)
        /// and move down from 3 to 2 to 1 character until a matching binary operation is found.
        /// Then, return that binary operation.
        /// </summary>
        /// <returns>An operator string, or null.</returns>
        public string? GobbleBinaryOp()
        {
            GobbleSpaces();

            var toCheck = Expression.SubstringSafe(Index, maxBinopLen);
            var tcLen = toCheck.Length;

            while (tcLen > 0)
            {
                // Don't accept a binary op when it is an identifier.
                // Binary ops that start with a identifier-valid character must be followed
                // by a non identifier-part valid character
                if (binaryOps.ContainsKey(toCheck) && (
                    !IsIdentifierStart(CharCode) ||
                    (Index + toCheck.Length < Expression.Length && !IsIdentifierPart(Expression.CharAt(Index + toCheck.Length)))
                ))
                {
                    Index += tcLen;
                    return toCheck;
                }
                toCheck = toCheck.SubstringSafe(0, --tcLen);
            }
            return null; // Port: Can't return false here like JavaScript does.
        }

        /// <summary>
        /// This function is responsible for gobbling an individual expression:<br />
        /// e.g. 
        ///   <c>1</c>,
        ///   <c>1+2</c>,
        ///   <c>a+(b*2)-Math.sqrt(2)</c>
        /// </summary>
        /// <returns>A SyntaxNode instance, or null.</returns>
        public SyntaxNode? GobbleBinaryExpression()
        {
            SyntaxNode? node;
            string? biop;
            double prec;
            List<SyntaxNode> stack;
            BinaryOpInfo biopInfo;
            SyntaxNode? left;
            SyntaxNode? right;
            int i;
            string curBiop;

            // First, try to get the leftmost thing.
            // Then, check to see if there's a binary operator operating on that leftmost thing.
            // Don't GobbleBinaryOp() without a left-hand-side.
            left = GobbleToken();
            if (left is null)
            {
                return left;
            }

            // If there wasn't a binary operator, just return the leftmost node.
            biop = GobbleBinaryOp();
            if (biop is null)
            {
                return left;
            }

            // Otherwise, we need to start a stack to properly place the binary operations in their
            // precedence structure.
            biopInfo = new BinaryOpInfo(biop, BinaryPrecedence(biop), rightAssociative.Contains(biop));

            right = GobbleToken();
            if (right is null)
            {
                throw Error($"Expected expression after {biop}");
            }

            stack = [left!, biopInfo, right!];

            biop = GobbleBinaryOp();

            while (biop is not null)
            {
                prec = BinaryPrecedence(biop);
                if (prec == 0)
                {
                    Index -= biop.Length;
                    break;
                }

                biopInfo = new BinaryOpInfo(biop, prec, rightAssociative.Contains(biop));

                curBiop = biop;

                bool comparePrev(BinaryOpInfo prev)
                {
                    return biopInfo.RightAssociative && prev.RightAssociative ?
                        prec > prev.Precision :
                        prec <= prev.Precision;
                }

                // Enforce order of operations based on operator precedence.
                while ((stack.Count > 2) && comparePrev((BinaryOpInfo)stack[^2]))
                {
                    right = stack.Pop();
                    biop = stack.PopAndCast<SyntaxNode, BinaryOpInfo>()!.Value;
                    left = stack.Pop();
                    node = new BinaryNode(biop, left, right);
                    stack.Add(node);
                }

                node = GobbleToken();
                if (node is null)
                {
                    throw Error($"Expected expression after {curBiop}");
                }

                stack.Add(biopInfo);
                stack.Add(node);

                // Port: Changed while() loop to avoid assignment.
                biop = GobbleBinaryOp();
            }

            i = stack.Count - 1;
            node = stack[i];

            while (i > 1)
            {
                node = new BinaryNode(((BinaryOpInfo)stack[i - 1]).Value!, stack[i - 2], node);
                i -= 2;
            }

            return node;
        }

        /// <summary>
        /// Fired before token is parsed.
        /// If the Node property on the event argument is set, standard token parsing is skipped.
        /// </summary>
        public event Action<NodeEvent>? BeforeToken; // Port: 'gobble-token' hook.
        SyntaxNode? OnBeforeToken()
        {
            if (BeforeToken is null) return null;

            var eventArgs = new NodeEvent();
            BeforeToken(eventArgs);
            return eventArgs.Node;
        }

        /// <summary>
        /// Fired after token parsing finishes.
        /// Each event subscriber is supplied a node that can be inspected or replaced.
        /// </summary>
        public event Action<NodeEvent>? AfterToken; // Port: 'after-token' hook.
        SyntaxNode? OnAfterToken(SyntaxNode? node)
        {
            if (AfterToken is not null)
            {
                var eventArgs = new NodeEvent(node);
                AfterToken(eventArgs);
                node = eventArgs.Node ?? node;
            }

            return node;
        }

        /// <summary>
        /// An individual part of a binary expression:<br />
        /// e.g. 
        ///   <c>foo.bar(baz)</c>,
        ///   <c>1</c>,
        ///   <c>"abc"</c>,
        ///   <c>(a % 2)</c> (because it's in parenthesis)
        /// </summary>
        /// <returns>A SyntaxNode instance, or null.</returns>
        public SyntaxNode? GobbleToken()
        {
            char ch;
            string toCheck;
            int tcLen;
            SyntaxNode? node;

            GobbleSpaces();
            node = OnBeforeToken();
            if (node is not null)
            {
                return OnAfterToken(node);
            }

            ch = CharCode;

#if NET8_0_OR_GREATER
            bool isNumLiteral = char.IsAsciiDigit(ch) || ch == PERIOD_CODE;
#else
            bool isNumLiteral = IsCharAsciiDigit(ch) || ch == PERIOD_CODE;
#endif
            if (isNumLiteral)
            {
                // Periods/dots ('.') can start off a numeric literal
                return GobbleNumericLiteral();
            }

            if (ch == SQUOTE_CODE || ch == DQUOTE_CODE)
            {
                // Single or double quotes
                node = GobbleStringLiteral();
            }
            else if (ch == OBRACK_CODE)
            {
                node = GobbleArray();
            }
            else
            {
                toCheck = Expression.SubstringSafe(Index, maxUnopLen);
                tcLen = toCheck.Length;

                while (tcLen > 0)
                {
                    // Don't accept an unary op when it is an identifier.
                    // Unary ops that start with a identifier-valid character must be followed
                    // by a non identifier-part valid character.
                    if (unaryOps.ContainsKey(toCheck) && (
                        !IsIdentifierStart(CharCode) ||
                        (Index + toCheck.Length < Expression.Length && !IsIdentifierPart(Expression.CharAt(Index + toCheck.Length)))
                    ))
                    {
                        Index += tcLen;
                        var argument = GobbleToken();

                        // If you need support for pre/postfix decrement/increment operations (x++, --x),
                        // please include the Assignment plugin. Assignment is not supported without it, by design.
                        if (AllowMissingUnaryArgument)
                        {
                            return argument is null ?
                                null :
                                OnAfterToken(new UnaryNode(toCheck, argument, true));
                        }

                        return argument is null ?
                            throw Error("Missing unaryOp argument") :
                            OnAfterToken(new UnaryNode(toCheck, argument, true));
                    }

                    toCheck = toCheck.SubstringSafe(0, --tcLen);
                }

                if (IsIdentifierStart(ch))
                {
                    var identNode = GobbleIdentifier();
                    node = identNode;

                    if (literals.TryGetValue(identNode.Name!, out object? value))
                    {
                        node = new LiteralNode(value, identNode.Name!);
                    }
                    else if (identNode.Name == ThisString)
                    {
                        node = new ThisNode();
                    }
                }
                else if (ch == OPAREN_CODE) // open parenthesis
                {
                    node = GobbleGroup();
                }
            }

            if (node is null)
            {
                return OnAfterToken(null);
            }

            node = GobbleTokenProperty(node);
            return OnAfterToken(node);
        }

        /// <summary>
        /// Gobble properties of of identifiers/strings/arrays/groups.<br />
        /// e.g. 
        ///   <c>foo</c>,
        ///   <c>bar.baz</c>,
        ///   <c>foo['bar'].baz</c>
        ///   <br /><br />
        /// It also gobbles function calls:<br />
        /// e.g.
        ///   <c>Math.acos(obj.angle)</c>
        /// </summary>
        /// <param name="node">The SyntaxNode respresenting the owner of this property.</param>
        /// <returns>A SyntaxNode instance.</returns>
        public SyntaxNode GobbleTokenProperty(SyntaxNode node)
        {
            GobbleSpaces();

            var ch = CharCode;

            while (ch == PERIOD_CODE || ch == OBRACK_CODE || ch == OPAREN_CODE || ch == QUMARK_CODE)
            {
                bool optional = false;
                if (ch == QUMARK_CODE)
                {
                    if (Expression.CharAt(Index + 1) != PERIOD_CODE)
                    {
                        break;
                    }

                    optional = true;
                    Index += 2;
                    GobbleSpaces();
                    ch = CharCode;
                }
                Index++;

                if (ch == OBRACK_CODE)
                {
                    node = new MemberNode(true, node, GobbleExpression());
                    GobbleSpaces();
                    ch = CharCode;
                    if (ch != CBRACK_CODE)
                    {
                        throw Error("Unclosed [");
                    }
                    Index++;
                }
                else if (ch == OPAREN_CODE)
                {
                    // A function call is being made; gobble all the arguments
                    node = new CallNode(node, GobbleArguments(CPAREN_CODE));
                }
                else if (ch == PERIOD_CODE || optional)
                {
                    if (optional)
                    {
                        Index--;
                    }

                    GobbleSpaces();

                    node = new MemberNode(false, node, GobbleIdentifier());
                }

                if (optional)
                {
                    node.Optional = true;
                }

                GobbleSpaces();
                ch = CharCode;
            }

            return node;
        }

        /// <summary>
        /// Reads digit characters to a string builder, until none are found.
        /// </summary>
        /// <param name="sb">The string builder to populate.</param>
        public void ReadDigitsToBuilder(StringBuilder sb)
        {
#if NET8_0_OR_GREATER
            while (char.IsAsciiDigit(CharCode)) // exponent itself
            {
                sb.Append(Expression.CharAt(Index++));
            }
#else
            while (IsCharAsciiDigit(CharCode)) // exponent itself
            {
                sb.Append(Expression.CharAt(Index++));
            }
#endif
        }

        /// <summary>
        /// Parse simple numeric literals: `12`, `3.4`, `.5`.
        /// Do this by using a string to keep track of everything in the numeric literal and 
        /// then calling `ParseFloat` on that string.
        /// </summary>
        /// <returns>A LiteralNode instance.</returns>
        public LiteralNode GobbleNumericLiteral()
        {
            return GobbleNumericLiteral(ReadDigitsToBuilder);
        }

        /// <summary>
        /// Parse simple numeric literals: `12`, `3.4`, `.5`.
        /// Do this by using a string to keep track of everything in the numeric literal and 
        /// then calling `ParseFloat` on that string.
        /// </summary>
        /// <param name="digitBuilder"></param>
        /// <returns>A LiteralNode instance.</returns>
        public LiteralNode GobbleNumericLiteral(Action<StringBuilder> digitBuilder)
        {
            int startIndex = Index;
            StringBuilder number = new();
            char ch;

            digitBuilder(number);

            if (CharCode == PERIOD_CODE) // can start with a decimal marker
            {
                number.Append(Expression.CharAt(Index++));

                digitBuilder(number);
            }

            ch = CharCode;
            if (ch == 'e' || ch == 'E') // exponent marker
            {
                number.Append(Expression.CharAt(Index++));
                ch = CharCode;

                if (ch == '+' || ch == '-') // exponent sign
                {
                    number.Append(Expression.CharAt(Index++));
                }

                // exponent itself
                digitBuilder(number);

#if NET8_0_OR_GREATER
                bool noExpectedExpo = !char.IsAsciiDigit(Expression.CharAt(Index - 1));
#else
                bool noExpectedExpo = !IsCharAsciiDigit(Expression.CharAt(Index - 1));
#endif
                if (noExpectedExpo)
                {
                    throw Error($"Expected exponent ({number}{CharCode})");
                }
            }

            // Check to make sure this isn't a variable name that start with a number (123abc)
            var chCode = CharCode;
            var numberText = number.ToString();
            if (IsIdentifierStart(chCode))
            {
                throw Error($"Variable names cannot start with a number ({numberText}{chCode})");
            }
            else if (chCode == PERIOD_CODE || (number.Length == 1 && numberText.CharAt(0) == PERIOD_CODE))
            {
                throw Error("Unexpected period");
            }

            return new LiteralNode(ParseFloat(numberText), Expression.FromTo(startIndex, Index));
        }

        /// <summary>
        /// Parses a string literal, staring with single or double quotes with basic support for escape codes:<br />
        /// e.g.
        ///   <c>"hello world"</c>,
        ///   <c>'this is\nJSEP'</c>
        /// </summary>
        /// <returns>A LiteralNode instance.</returns>
        public LiteralNode GobbleStringLiteral()
        {
            StringBuilder str = new();
            int startIndex = Index;
            char quote = Expression.CharAt(Index++);
            bool closed = false;

            while (Index < Expression.Length)
            {
                var ch = Expression.CharAt(Index++);

                if (ch == quote)
                {
                    closed = true;
                    break;
                }
                else if (ch == '\\')
                {
                    ch = Expression.CharAt(Index++);
                    str.Append(ReplaceEscapeChar(ch));
                }
                else
                {
                    str.Append(ch);
                }
            }

            var strText = str.ToString();
            if (!closed)
            {
                throw Error($"Unclosed quote after \"{strText}\"");
            }

            return new LiteralNode(strText, Expression.FromTo(startIndex, Index));
        }

        /// <summary>
        /// Gobbles only identifiers:<br />
        /// e.g.
        ///   <c>foo</c>,
        ///   <c>_value</c>,
        ///   <c>$x1</c>
        /// </summary>
        /// <returns></returns>
        public IdentifierNode GobbleIdentifier()
        {
            char ch = CharCode;
            int start = Index;

            if (IsIdentifierStart(ch))
            {
                Index++;
            }
            else if (ch == '\0')
            {
                // Port: This is not present in the JavaScript version (due to \0 behavior difference).
                throw Error($"No identifier");
            }
            else
            {
                throw Error($"Unexpected {ch}");
            }

            while (Index < Expression.Length)
            {
                ch = CharCode;

                if (IsIdentifierPart(ch))
                {
                    Index++;
                }
                else
                {
                    break;
                }
            }

            return new IdentifierNode(Expression.FromTo(start, Index));
        }

        /// <summary>
        /// Gobbles a list of arguments within the context of a function call
        /// or array literal. This function also assumes that the opening character
        /// <c>(</c> or <c>[</c> has already been gobbled, and gobbles expressions and commas
        /// until the terminator character <c>)</c> or <c>]</c> is encountered:<br />
        /// e.g. 
        ///   <c>foo(bar, baz)</c>,
        ///   <c>my_func()</c>, or
        ///   <c>[bar, baz]</c>
        /// </summary>
        /// <param name="termination">The terminating characer to stop the processing of arguments.</param>
        /// <returns>A list of zero or more SyntaxNodes. May have null entries.</returns>
        public List<SyntaxNode?> GobbleArguments(char termination)
        {
            List<SyntaxNode?> args = [];
            bool closed = false;
            int separatorCount = 0;

            while (Index < Expression.Length)
            {
                GobbleSpaces();
                var chi = CharCode;

                if (chi == termination) // done parsing
                {
                    closed = true;
                    Index++;

                    if (termination == CPAREN_CODE && separatorCount > 0 && separatorCount >= args.Count)
                    {
                        throw Error($"Unexpected token {termination}");
                    }

                    break;
                }
                else if (chi == COMMA_CODE) // between expressions
                {
                    Index++;
                    separatorCount++;

                    if (separatorCount != args.Count) // missing argument
                    {
                        if (termination == CPAREN_CODE)
                        {
                            throw Error("Unexpected token ,");
                        }
                        else if (termination == CBRACK_CODE)
                        {
                            for (var arg = args.Count; arg < separatorCount; arg++)
                            {
                                args.Add(null);
                            }
                        }
                    }
                }
                else if (args.Count != separatorCount && separatorCount != 0)
                {
                    // NOTE: `&& separator_count !== 0` allows for either all commas, or all spaces as arguments.
                    throw Error("Expected comma");
                }
                else
                {
                    var node = GobbleExpression();

                    if (node is null || node is CompoundNode)
                    {
                        throw Error("Expected comma");
                    }

                    args.Add(node);
                }
            }

            if (!closed)
            {
                throw Error($"Expected {termination}");
            }

            return args;
        }

        /// <summary>
        /// Responsible for parsing a group of things within parentheses `()`
        /// that have no identifier in front (so not a function call).
        /// This function assumes that it needs to gobble the opening parenthesis and
        /// then tries to gobble everything within that parenthesis,
        /// assuming that the next thing it should see is the close parenthesis. 
        /// If not, then the expression probably doesn't have a `)`.
        /// </summary>
        /// <returns>A SyntaxNode instance.</returns>
        public SyntaxNode GobbleGroup()
        {
            Index++;
            var nodes = GobbleExpressions(CPAREN_CODE);

            if (CharCode == CPAREN_CODE)
            {
                Index++;

                if (nodes.Count == 1)
                {
                    return nodes[0];
                }
                else
                {
                    return new SequenceNode(nodes!);
                }
            }

            throw Error("Unclosed (");
        }

        /// <summary>
        /// Responsible for parsing Array literals `[1, 2, 3]`
        /// This function assumes that it needs to gobble the opening bracket
        /// and then tries to gobble the expressions as arguments.
        /// </summary>
        /// <remarks>
        /// Invokes GobbleArguments() with a ending bracket (`]`) as terminator.
        /// </remarks>
        /// <returns>An ArrayNode instance.</returns>
        public ArrayNode GobbleArray()
        {
            Index++;

            return new ArrayNode(GobbleArguments(CBRACK_CODE));
        }
    }
}
