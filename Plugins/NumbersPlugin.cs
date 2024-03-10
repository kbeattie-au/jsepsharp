using JsepSharp.Extensions;
using JsepSharp.SyntaxTree;
using System.Text;

namespace JsepSharp.Plugins
{
    /// <summary>
    /// Adds support for hexadecimal, octal, and binary number formats. 
    /// It also adds support for ignoring underscore characters in numbers.
    /// </summary>
    public sealed class NumbersPlugin : Plugin
    {
        static readonly int[] indices = [0, 1, 2];

        /// <inheritdoc />
        public override string Name => "Numbers";

#if NET8_0_OR_GREATER
        record RangePair(char Min, char Max);
#else
        struct RangePair(char min, char max)
        {
            public char Min = min;
            public char Max = max;
        }
#endif

        /// <inheritdoc />
        public NumbersPlugin(Jsep parser) : base(parser)
        {
            parser.BeforeToken += Parser_BeforeToken;
        }

        // Port: 'gobble-token' hook.
        void Parser_BeforeToken(NodeEvent env)
        {
            var node = GobbleNumber();
            if (node is null) return;

            env.Node = node;
        }

        void ReadDigitsToBuilderIgnoreUnderscore(StringBuilder number)
        {
            var parser = Parser;
            char ch = parser.CharCode;

            while (true)
            {
#if NET8_0_OR_GREATER
                var isDigit = char.IsAsciiDigit(ch);
#else
                var isDigit = Jsep.IsCharAsciiDigit(ch);
#endif

                if (isDigit)
                {
                    number.Append(ch);
                }
                else if (ch != Jsep.UNDERSCORE_CODE)
                {
                    break;
                }

                parser.Index++;
                ch = parser.CharCode;
            }
        }

        List<RangePair>? GetNumberCodeRanges(char numType)
        {
            if (numType == 'x' || numType == 'X')
            {
                Parser.Index += 2;
                return [
                    new RangePair('0', '9'),
                    new RangePair('A', 'F'),
                    new RangePair('a', 'f')
                ];
            }
            
            if (numType == 'b' || numType == 'B')
            {
                Parser.Index += 2;
                return [new RangePair('0', '1')];
            }
            
            if (numType == 'o' || numType == 'O' )
            {
                Parser.Index += 2;
                return [new RangePair('0', '7')];
            }

            // 0-7 allows non-standard 0644 = 420
            if (numType == '0' || numType == '7')
            {
                Parser.Index += 1;
                return [new RangePair('0', '7')];
            }

            return null;
        }

        static int GetNumberBase(char numType)
        {
            if (numType == 'x' || numType == 'X')
            {
                return 16;
            }
            else if (numType == 'b' || numType == 'B')
            {
                return 2;
            }

            // default (includes non-stand 044)
            // code path not used for base-10 numbers.
            return 8;
        }

        static bool IsUnderscoreOrWithinRange(char code, List<RangePair> ranges)
        {
            return code == Jsep.UNDERSCORE_CODE || ranges.Any(c => code >= c.Min && code <= c.Max);
        }

        LiteralNode? GobbleNumber()
        {
            var ch = Parser.CharCode;

            if (ch == Jsep.NUM_0_CODE)
            {
                var startIndex = Parser.Index;
                var numType = Parser.Expression.CharAt(Parser.Index + 1);
                var ranges = GetNumberCodeRanges(numType);
                if (ranges is null)
                {
                    return null;
                }

                StringBuilder number = new();
                while (IsUnderscoreOrWithinRange(Parser.CharCode, ranges))
                {
                    if (Parser.CharCode == Jsep.UNDERSCORE_CODE)
                    {
                        Parser.Index++;
                    }
                    else
                    {
                        number.Append(Parser.Expression.CharAt(Parser.Index++));
                    }
                }

                // confirm valid syntax after building number string within ranges
                ch = Parser.CharCode;
                if (Jsep.IsIdentifierPart(ch))
                {
                    if (Jsep.IsCharAsciiDigit(ch) && Jsep.IsCharAsciiDigit(numType))
                    {
                        // abort octal processing and reset to ignore the leading 0
                        Parser.Index = startIndex + 1;
                        var fallback = Parser.GobbleNumericLiteral(ReadDigitsToBuilderIgnoreUnderscore);
                        return new LiteralNode(fallback.Value, $"0{fallback.Raw}", fallback.Optional);
                    }

                    throw Parser.Error("unexpected char within number");
                }

                decimal val;
                try
                {
                    val = Convert.ToInt64(number.ToString(), GetNumberBase(numType));
                }
                catch
                {
                    throw Parser.Error("number could not be parsed");
                }

                return new LiteralNode(val, Parser.Expression.FromTo(startIndex, Parser.Index));
            }
            else if (Jsep.IsCharAsciiDigit(ch) || ch == Jsep.PERIOD_CODE)
            {
                return Parser.GobbleNumericLiteral(ReadDigitsToBuilderIgnoreUnderscore);
            }

            return null;
        }
    }
}
