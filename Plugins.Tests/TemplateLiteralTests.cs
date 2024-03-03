using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins.Tests
{
    [TestClass]
    public class TemplateLiteralTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(TernaryPlugin), typeof(TemplateLiteralPlugin) });
        }

        [TestMethod]
        public void TestBasicTemplate()
        {
            var expr = @"`hi ${name}`";

            var actual = Parse(expr);
            var expected = TLit(
                [TElem("hi ", "hi ", false), TElem("", "", true)], 
                [Id("name")]);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestTaggedNestedTemplate()
        {
            var expr = @"abc`token ${`nested ${`deeply` + ""str""} blah`}`";

            var actual = Parse(expr);
            var expected = TagLit(
                Id("abc"), 
                TLit(
                    [
                        TElem("token ", "token ", false),
                        TElem("", "", true)
                    ],
                    [
                        TLit(
                            [
                                TElem("nested ", "nested ", false),
                                TElem(" blah", " blah", true)
                            ],
                            [
                                Bin("+",
                                    TLit(
                                        [TElem("deeply", "deeply", true)],
                                        []),
                                    Lit("str", "\"str\""))
                            ])
                    ]));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMemberTaggedNestedTemplate()
        {
            var expr = @"String.raw`token ${`nested ${`deeply` + ""str""} blah`}`";

            var actual = Parse(expr);
            var expected = TagLit(
                Member(false,
                    Id("String"),
                    Id("raw")),
                TLit(
                    [
                        TElem("token ", "token ", false), 
                        TElem("", "", true)
                    ],
                    [
                        TLit(
                            [
                                TElem("nested ", "nested ", false),
                                TElem(" blah", " blah", true)
                            ],
                            [
                                Bin("+",
                                    TLit(
                                    [
                                        TElem("deeply", "deeply", true)
                                    ], []),
                                    Lit("str", "\"str\""))
                            ])
                    ]));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultipleVariablesInTemplate()
        {
            var expr = @"`hi ${last}, ${first} ${middle}!`";

            var actual = Parse(expr);
            var expected = TLit(
                [
                    TElem("hi ", "hi ", false),
                    TElem(", ", ", ", false),
                    TElem(" ", " ", false),
                    TElem("!", "!", true)
                ],
                [
                    Id("last"),
                    Id("first"),
                    Id("middle")
                ]);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCookedEscapedCharsInTemplate()
        {
            var expr = "`hi\\n\t`";

            var actual = Parse(expr);
            var expected = TLit([TElem("hi\\n\t", "hi\n\t", true)], []);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestTernaryTemplate()
        {
            var expr = "`a\nbc${ b ? 1 : 2 }`";

            var actual = Parse(expr);
            var expected = TLit(
                [
                    TElem("a\nbc", "a\nbc", false),
                    TElem("", "", true)
                ],
                [
                    IIf(
                        Id("b"),
                        Lit(1, "1"),
                        Lit(2, "2"))
                ]);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParserErrors()
        {
            Exception ex;

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("`abc ${ `"),
                "unclosed empty interpolation");
            Assert.AreEqual("Unclosed ` at character 9.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("abc`123"),
                "unclosed template string");
            Assert.AreEqual("Unclosed ` at character 7.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("abc`${a`"),
                "unclosed interpolation with content");
            Assert.AreEqual("Unclosed ` at character 8.", ex.Message);
        }
    }
}
