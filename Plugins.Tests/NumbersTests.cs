using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins.Tests
{
    [TestClass]
    public class NumbersTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(NumbersPlugin) });
        }

        [TestMethod]
        public void TestParseCorrectly()
        {
            (string, object)[] tuples = [
                (@"0xA", 10),
                (@"0xfF", 255),
                (@"0xA_a", 170),
                (@"0b01010101", 85),
                (@"0b0000_0111", 7),
                (@"0b1000_0000__0000_0000__0000_0000__0000_0000", 2147483648),
                (@"0755", 493),
                (@"0o644", 420),
                (@"0795", 795), // aborts octal, reverts to decimal
	            (@"115_200", 115200),
                (@"10_000E1", 10000E1),
                (@"1_0E2_0", 10E20),
                (@"1_0E+2_0", 10E20),
                (@"1E-2", 1e-2),
                (@"1_234.567_89", 1234.56789),
                (@"0.1", 0.1),
                (@".1", .1)
            ];

            foreach ((string raw, object val) in tuples)
            {
                var expected = Lit(val, raw);
                var actual = Parse(raw);

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestParserErrors()
        {
            string[] expressions = [
                "0b12abc",
                "123abc",
                "1_2_abc",
                "0755a",
                "0x1Z",
                "12_34.56.78",
                "ab_cd.123",
                "1_2E3.4"
            ];

            foreach (var expr in expressions)
            {
                Assert.ThrowsException<ParsingException>(() => Parse(expr), expr);
            }
        }

        [TestMethod]
        public void TestParseNoErrors()
        {
            string[] expressions = [
                "_123",
                "_0b123"
            ];

            foreach (var expr in expressions)
            {
                Parse(expr);
            }
        }
    }
}
