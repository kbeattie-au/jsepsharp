using JsepNet.SyntaxTree;
using Newtonsoft.Json;

namespace JsepNet.Tests
{
    [TestClass]
    public class JsepTests : AstBuilder
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr);
        }

        [TestMethod]
        public void TestConstants()
        {
            Assert.AreEqual(Lit(123, "123"), Parse("123"));
            Assert.AreEqual(Lit("abc", "\'abc\'"), Parse("\'abc\'"));
            Assert.AreEqual(Lit("abc", "\"abc\""), Parse("\"abc\""));
            Assert.AreEqual(Lit(12.3, "12.3"), Parse("12.3"));
            Assert.AreEqual(Lit(true, "true"), Parse("true"));
            Assert.AreEqual(Lit(false, "false"), Parse("false"));
            Assert.AreEqual(Lit(null, "null"), Parse("null"));
        }

        [TestMethod]
        public void TestLiteralStringEscapes()
        {
            string[][] rows = [
                ["'a \\w b'", "a w b"],
                ["'a \\' b'", "a ' b"],
                ["'a \\n b'", "a \n b"],
                ["'a \\r b'", "a \r b"],
                ["'a \\t b'", "a \t b"],
                ["'a \\b b'", "a \b b"],
                ["'a \\f b'", "a \f b"],
                ["'a \\v b'", "a \v b"],
                ["'a \\\\ b'", "a \\ b"]
            ];

            foreach (var r in rows)
            {
                Assert.AreEqual(Lit(r[1], r[0]), Parse(r[0]));
            }
        }

        [TestMethod]
        public void TestVariables()
        {
            SyntaxNode actual, expected;

            Assert.AreEqual(Id("abc"), Parse("abc"));
            Assert.AreEqual(Id("Δέλτα"), Parse("Δέλτα"));

            actual = Parse("a.b.c");
            expected = Member(false, Member(false, Id("a"), Id("b")), Id("c"));
            Assert.AreEqual(expected, actual);

            actual = Parse("a.b[c[0]]");
            expected = Member(
                true,
                Member(false, Id("a"), Id("b")),
                Member(true, Id("c"), Lit(0D, "0")));
            Assert.AreEqual(expected, actual);

            actual = Parse("a?.b?.(arg)?.[c] ?. d");
            expected = Member(
                false,
                Member(
                    true,
                    Call(
                        Member(false, Id("a"), Id("b"), true),
                        [Id("arg")],
                        true),
                    Id("c"),
                    true),
                Id("d"),
                true);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFunctionCalls()
        {
            SyntaxNode actual, expected;

            actual = Parse("a(b, c(d,e), f)");
            expected = Call(Id("a"), [
                Id("b"),
                Call(Id("c"), [Id("d"), Id("e")]),
                Id("f"),
            ]);
            Assert.AreEqual(expected, actual);

            actual = Parse("a b + c");
            expected = Comp(Id("a"), Bin("+", Id("b"), Id("c")));
            Assert.AreEqual(expected, actual);

            actual = Parse("'a'.toString()");
            expected = Call(Member(false, Lit("a", "'a'"), Id("toString")));
            Assert.AreEqual(expected, actual);

            actual = Parse("[1].length");
            expected = Member(false, Ar(Lit(1D, "1")), Id("length"));
            Assert.AreEqual(expected, actual);

            actual = Parse(";");
            expected = Comp();
            Assert.AreEqual(expected, actual);

            // allow all spaces or all commas to separate arguments
            actual = Parse("check(a, b, c, d)");
            expected = Call(Id("check"), [Id("a"), Id("b"), Id("c"), Id("d")]);
            Assert.AreEqual(expected, actual);

            actual = Parse("check(a b c d)");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestArrays()
        {
            SyntaxNode actual, expected;

            actual = Parse("[]");
            expected = Ar();
            Assert.AreEqual(expected, actual);

            actual = Parse("[a]");
            expected = Ar(Id("a"));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestThis()
        {
            SyntaxNode actual, expected;

            actual = Parse("this");
            expected = Self();
            Assert.AreEqual(expected, actual);

            actual = Parse("this.prop");
            expected = Member(false, Self(), Id("prop"));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestOperators()
        {
            SyntaxNode actual, expected;

            actual = Parse("-1");
            expected = Un("-", Lit(1, "1"), true);
            Assert.AreEqual(expected, actual);

            actual = Parse("1+2");
            expected = Bin("+", Lit(1, "1"), Lit(2, "2"));
            Assert.AreEqual(expected, actual);

            actual = Parse("1*2");
            expected = Bin("*", Lit(1, "1"), Lit(2, "2"));
            Assert.AreEqual(expected, actual);

            actual = Parse("1*(2+3)");
            expected = Bin("*", Lit(1, "1"), Bin("+", Lit(2, "2"), Lit(3, "3")));
            Assert.AreEqual(expected, actual);

            actual = Parse("(1+2)*3+4-2-5+2/2*3");
            expected = 
                Bin("+",
                    Bin("-",
                        Bin("-",
                            Bin("+",
                                Bin("*",
                                    Bin("+", Lit(1, "1"), Lit(2, "2")),
                                    Lit(3, "3")),
                                Lit(4, "4")),
                            Lit(2, "2")),
                        Lit(5, "5")),
                    Bin("*",
                        Bin("/", Lit(2, "2"), Lit(2, "2")),
                        Lit(3, "3")));
            Assert.AreEqual(expected, actual);

            actual = Parse("1 + 2-   3*\t4 /8");
            expected = 
                Bin("-",
                    Bin("+", Lit(1, "1"), Lit(2, "2")),
                    Bin("/",
                        Bin("*", Lit(3, "3"), Lit(4, "4")),
                        Lit(8, "8")));
            Assert.AreEqual(expected, actual);

            actual = Parse("\n1\r\n+\n2\n");
            expected = Bin("+", Lit(1, "1"), Lit(2, "2"));
            Assert.AreEqual(expected, actual);

            actual = Parse("1 + -2");
            expected = Bin("+", Lit(1, "1"), Un("-", Lit(2, "2"), true));
            Assert.AreEqual(expected, actual);

            actual = Parse("-1 + -2 * -3 * 2");
            expected =
                Bin("+",
                    Un("-", Lit(1, "1"), true),
                    Bin("*",
                        Bin("*",
                            Un("-", Lit(2, "2"), true),
                            Un("-", Lit(3, "3"), true)),
                        Lit(2, "2")));
            Assert.AreEqual(expected, actual);

            try
            {
                // Add right-associative exponentation operator.
                Jsep.AddBinaryOp("**", 11, true);

                // `x ** y ** z` and `x ** (y ** z)` have the same result.
                actual = Parse("2 ** 3 ** 4");
                expected = Bin("**", Lit(2, "2"), Bin("**", Lit(3, "3"), Lit(4, "4")));
                Assert.AreEqual(expected, actual);

                actual = Parse("2 ** (3 ** 4)");
                expected = Bin("**", Lit(2, "2"), Bin("**", Lit(3, "3"), Lit(4, "4")));
                Assert.AreEqual(expected, actual);

                actual = Parse("2 ** 3 ** 4 * 5 ** 6 ** 7 * (8 + 9)");
                expected =
                    Bin("*",
                        Bin("*",
                            Bin("**",
                                Lit(2, "2"),
                                Bin("**", Lit(3, "3"), Lit(4, "4"))),
                            Bin("**",
                                Lit(5, "5"),
                                    Bin("**", Lit(6, "6"), Lit(7, "7")))),
                        Bin("+", Lit(8, "8"), Lit(9, "9")));
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                Jsep.RemoveBinaryOp("**");
            }
        }

        // Port: Copied 'Custom alphanumeric operators' into here for "and"
        // "or" was already coverered by the spec.
        [TestMethod]
        public void TestCustomOperators()
        {
            SyntaxNode? actual, expected;

            try
            {
                Jsep.AddBinaryOp("^", 10, false); // Changing existing built-in precedence.
                Jsep.AddBinaryOp("×", 9, false);  // Adding new.
                Jsep.AddBinaryOp("or", 1, false);
                Jsep.AddBinaryOp("and", 2, false);

                actual = Parse("a^b");
                expected = Bin("^", Id("a"), Id("b"));
                Assert.AreEqual(expected, actual);

                actual = Parse("a×b");
                expected = Bin("×", Id("a"), Id("b"));
                Assert.AreEqual(expected, actual);

                actual = Parse("a and b");
                expected = Bin("and", Id("a"), Id("b"));
                Assert.AreEqual(expected, actual);

                actual = Parse("a^b×c");
                expected = Bin("×", Bin("^", Id("a"), Id("b")), Id("c"));
                Assert.AreEqual(expected, actual);

                // `or` within a identifier should be ignored.
                actual = Parse("oneWord ordering anotherWord");
                expected = Comp(Id("oneWord"), Id("ordering"), Id("anotherWord"));
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                Jsep.ResetBinaryOps();
            }

            try
            {
                Jsep.AddUnaryOp("#");
                Jsep.AddUnaryOp("not");
                Jsep.AddUnaryOp("notes");

                actual = Parse("#a");
                expected = Un("#", Id("a"), true);
                Assert.AreEqual(expected, actual);

                actual = Parse("not a");
                expected = Un("not", Id("a"), true);
                Assert.AreEqual(expected, actual);

                actual = Parse("notes");
                expected = Id("notes");
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                Jsep.ResetUnaryOps();
            }
        }

        [TestMethod]
        public void TestCustomIdentifierCharacters()
        {
            try
            {
                Jsep.AddIdentifier('@');

                SyntaxNode? actual = Parse("@asd");
                SyntaxNode? expected = Id("@asd");
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                Jsep.ResetIdentifiers();
            }
        }

        [TestMethod]
        public void TestBadNumbers()
        {
            var ex = Assert.ThrowsException<ParsingException>(() => {
                Parse("1.2.3");
            });
            Assert.AreEqual("Unexpected period at character 3.", ex.Message);
        }

        [TestMethod]
        public void TestMissingArguments()
        {
            Exception ex;

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("check(,)"),
                "detects missing argument (all)");
            Assert.AreEqual("Unexpected token , at character 7.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("check(,1,2)"),
                "detects missing argument (head)");
            Assert.AreEqual("Unexpected token , at character 7.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("check(1,,2)"),
                "detects missing argument (intervening)");
            Assert.AreEqual("Unexpected token , at character 9.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("check(1,2,)"),
                "detects missing argument (tail)");
            Assert.AreEqual("Unexpected token ) at character 11.", ex.Message);

            // Port: Skipped some failing ones from the JavaScript implementation.
            // [https://ericsmekens.github.io/jsep/] has no issue parsing these either.
            // check(a, b c d)
            // check(a, b, c d)
            // check(a b, c, d)
            // check(a b c, d)
        }

        [TestMethod]
        public void TestIncompletedExpressions()
        {
            Exception ex;

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("myFunction(a,b"),
                "detects unfinished expression call");
            Assert.AreEqual("Expected ) at character 14.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("[1,2"),
                "detects unfinished array");
            Assert.AreEqual("Expected ] at character 4.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("-1+2-"),
                "detects trailing operator");
            Assert.AreEqual("Expected expression after - at character 5.", ex.Message);
        }

        [TestMethod]
        public void TestInvalidExpressions()
        {
            // Port: Excluded `()()` and `() + 1` because they pass.
            // Not sure they should, but the JS implementation permits those,
            // and this is trying to be pretty close to it, behavior-wise.
            string[] invalids = ["!", "*x", "||x", "."];

            foreach (var expr in invalids)
            {
                Assert.ThrowsException<ParsingException>(
                    () => Parse(expr),
                    $"should throw on invalid expr \"{expr}\"");
            }
        }

        // Port: Instead of comparing these with Esprima, this implementation
        // is comparing outputs against the JavaScript version. This also tests
        // that JSON deserialization of outputs maps to correct node classes.
        [TestMethod]
        public void TestJavascriptComparison()
        {
            // Generated from JsepNet.Tests.Generate/app.js
            Dictionary<string, string> jsonFromJs = new()
            {
                { @"[1,,3]", @"{""type"":""ArrayExpression"",""elements"":[{""type"":""Literal"",""value"":1,""raw"":""1""},null,{""type"":""Literal"",""value"":3,""raw"":""3""}]}" },
                { @"[1,,]", @"{""type"":""ArrayExpression"",""elements"":[{""type"":""Literal"",""value"":1,""raw"":""1""},null]}" },
                { @" true", @"{""type"":""Literal"",""value"":true,""raw"":""true""}" },
                { @"false ", @"{""type"":""Literal"",""value"":false,""raw"":""false""}" },
                { @" 1.2 ", @"{""type"":""Literal"",""value"":1.2,""raw"":""1.2""}" },
                { @" .2 ", @"{""type"":""Literal"",""value"":0.2,""raw"":"".2""}" },
                { @"a", @"{""type"":""Identifier"",""name"":""a""}" },
                { @"a .b", @"{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""Identifier"",""name"":""a""},""property"":{""type"":""Identifier"",""name"":""b""}}" },
                { @"a.b. c", @"{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""Identifier"",""name"":""a""},""property"":{""type"":""Identifier"",""name"":""b""}},""property"":{""type"":""Identifier"",""name"":""c""}}" },
                { @"a [b]", @"{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""Identifier"",""name"":""a""},""property"":{""type"":""Identifier"",""name"":""b""}}" },
                { @"a.b  [ c ] ", @"{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""Identifier"",""name"":""a""},""property"":{""type"":""Identifier"",""name"":""b""}},""property"":{""type"":""Identifier"",""name"":""c""}}" },
                { @"$foo[ bar][ baz].other12 ['lawl'][12]", @"{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""Identifier"",""name"":""$foo""},""property"":{""type"":""Identifier"",""name"":""bar""}},""property"":{""type"":""Identifier"",""name"":""baz""}},""property"":{""type"":""Identifier"",""name"":""other12""}},""property"":{""type"":""Literal"",""value"":""lawl"",""raw"":""'lawl'""}},""property"":{""type"":""Literal"",""value"":12,""raw"":""12""}}" },
                { @"$foo     [ 12   ] [ baz[z]    ].other12*4 + 1 ", @"{""type"":""BinaryExpression"",""operator"":""+"",""left"":{""type"":""BinaryExpression"",""operator"":""*"",""left"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""Identifier"",""name"":""$foo""},""property"":{""type"":""Literal"",""value"":12,""raw"":""12""}},""property"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""Identifier"",""name"":""baz""},""property"":{""type"":""Identifier"",""name"":""z""}}},""property"":{""type"":""Identifier"",""name"":""other12""}},""right"":{""type"":""Literal"",""value"":4,""raw"":""4""}},""right"":{""type"":""Literal"",""value"":1,""raw"":""1""}}" },
                { @"$foo[ bar][ baz]    (a, bb ,   c  )   .other12 ['lawl'][12]", @"{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""CallExpression"",""arguments"":[{""type"":""Identifier"",""name"":""a""},{""type"":""Identifier"",""name"":""bb""},{""type"":""Identifier"",""name"":""c""}],""callee"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""Identifier"",""name"":""$foo""},""property"":{""type"":""Identifier"",""name"":""bar""}},""property"":{""type"":""Identifier"",""name"":""baz""}}},""property"":{""type"":""Identifier"",""name"":""other12""}},""property"":{""type"":""Literal"",""value"":""lawl"",""raw"":""'lawl'""}},""property"":{""type"":""Literal"",""value"":12,""raw"":""12""}}" },
                { @"(a(b(c[!d]).e).f+'hi'==2) === true", @"{""type"":""BinaryExpression"",""operator"":""==="",""left"":{""type"":""BinaryExpression"",""operator"":""=="",""left"":{""type"":""BinaryExpression"",""operator"":""+"",""left"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""CallExpression"",""arguments"":[{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""CallExpression"",""arguments"":[{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""Identifier"",""name"":""c""},""property"":{""type"":""UnaryExpression"",""operator"":""!"",""argument"":{""type"":""Identifier"",""name"":""d""},""prefix"":true}}],""callee"":{""type"":""Identifier"",""name"":""b""}},""property"":{""type"":""Identifier"",""name"":""e""}}],""callee"":{""type"":""Identifier"",""name"":""a""}},""property"":{""type"":""Identifier"",""name"":""f""}},""right"":{""type"":""Literal"",""value"":""hi"",""raw"":""'hi'""}},""right"":{""type"":""Literal"",""value"":2,""raw"":""2""}},""right"":{""type"":""Literal"",""value"":true,""raw"":""true""}}" },
                { @"(1,2)", @"{""type"":""SequenceExpression"",""expressions"":[{""type"":""Literal"",""value"":1,""raw"":""1""},{""type"":""Literal"",""value"":2,""raw"":""2""}]}" },
                { @"(a, a + b > 2)", @"{""type"":""SequenceExpression"",""expressions"":[{""type"":""Identifier"",""name"":""a""},{""type"":""BinaryExpression"",""operator"":"">"",""left"":{""type"":""BinaryExpression"",""operator"":""+"",""left"":{""type"":""Identifier"",""name"":""a""},""right"":{""type"":""Identifier"",""name"":""b""}},""right"":{""type"":""Literal"",""value"":2,""raw"":""2""}}]}" },
                { @"(((1)))", @"{""type"":""Literal"",""value"":1,""raw"":""1""}" },
                { @"(Object.variable.toLowerCase()).length == 3", @"{""type"":""BinaryExpression"",""operator"":""=="",""left"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""CallExpression"",""arguments"":[],""callee"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""Identifier"",""name"":""Object""},""property"":{""type"":""Identifier"",""name"":""variable""}},""property"":{""type"":""Identifier"",""name"":""toLowerCase""}}},""property"":{""type"":""Identifier"",""name"":""length""}},""right"":{""type"":""Literal"",""value"":3,""raw"":""3""}}" },
                { @"(Object.variable.toLowerCase())  .  length == 3", @"{""type"":""BinaryExpression"",""operator"":""=="",""left"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""CallExpression"",""arguments"":[],""callee"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""Identifier"",""name"":""Object""},""property"":{""type"":""Identifier"",""name"":""variable""}},""property"":{""type"":""Identifier"",""name"":""toLowerCase""}}},""property"":{""type"":""Identifier"",""name"":""length""}},""right"":{""type"":""Literal"",""value"":3,""raw"":""3""}}" },
                { @"[1] + [2]", @"{""type"":""BinaryExpression"",""operator"":""+"",""left"":{""type"":""ArrayExpression"",""elements"":[{""type"":""Literal"",""value"":1,""raw"":""1""}]},""right"":{""type"":""ArrayExpression"",""elements"":[{""type"":""Literal"",""value"":2,""raw"":""2""}]}}" },
                { @"""a""[0]", @"{""type"":""MemberExpression"",""computed"":true,""object"":{""type"":""Literal"",""value"":""a"",""raw"":""\""a\""""},""property"":{""type"":""Literal"",""value"":0,""raw"":""0""}}" },
                { @"[1](2)", @"{""type"":""CallExpression"",""arguments"":[{""type"":""Literal"",""value"":2,""raw"":""2""}],""callee"":{""type"":""ArrayExpression"",""elements"":[{""type"":""Literal"",""value"":1,""raw"":""1""}]}}" },
                { @"""a"".length", @"{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""Literal"",""value"":""a"",""raw"":""\""a\""""},""property"":{""type"":""Identifier"",""name"":""length""}}" },
                { @"a.this", @"{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""Identifier"",""name"":""a""},""property"":{""type"":""Identifier"",""name"":""this""}}" },
                { @"a.true", @"{""type"":""MemberExpression"",""computed"":false,""object"":{""type"":""Identifier"",""name"":""a""},""property"":{""type"":""Identifier"",""name"":""true""}}" },
            };

            Dictionary<string, SyntaxNode> expectedNodes = new()
            {
                { @"[1,,3]", Ar(Lit(1, "1"), null, Lit(3, "3")) },
                { @"[1,,]", Ar(Lit(1, "1"), null) },
                { @" true", Lit(true, "true") },
                { @"false ", Lit(false, "false") },
                { @" 1.2 ", Lit(1.2, "1.2") },
                { @" .2 ", Lit(0.2, ".2") },
                { @"a", Id("a") },
                { @"a .b", Member(false, Id("a"), Id("b")) },
                { @"a.b. c", Member(false, Member(false, Id("a"), Id("b")), Id("c")) },
                { @"a [b]", Member(true, Id("a"), Id("b")) },
                { @"a.b  [ c ] ", Member(true, Member(false, Id("a"), Id("b")), Id("c")) },
                {
                    @"$foo[ bar][ baz].other12 ['lawl'][12]",
                    Member(true,
                        Member(true,
                            Member(false,
                                Member(true,
                                    Member(true, Id("$foo"), Id("bar")), 
                                    Id("baz")),
                                Id("other12")),
                            Lit("lawl", "'lawl'")),
                        Lit(12, "12"))
                },
                {
                    @"$foo     [ 12   ] [ baz[z]    ].other12*4 + 1 ",
                    Bin("+", 
                        Bin("*",
                            Member(false, 
                                Member(true,
                                    Member(true, Id("$foo"), Lit(12, "12")),
                                    Member(true, Id("baz"), Id("z"))),
                                Id("other12")),
                            Lit(4, "4")),
                        Lit(1, "1"))
                },
                {
                    @"$foo[ bar][ baz]    (a, bb ,   c  )   .other12 ['lawl'][12]",
                    Member(true, 
                        Member(true, 
                            Member(false, 
                                Call(
                                    Member(true,
                                        Member(true, Id("$foo"), Id("bar")),
                                        Id("baz")),
                                    [Id("a"), Id("bb"), Id("c")]),
                                Id("other12")),
                            Lit("lawl", "'lawl'")),
                        Lit(12, "12"))
                },
                {
                    @"(a(b(c[!d]).e).f+'hi'==2) === true",
                    Bin("===", 
                        Bin("==", 
                            Bin("+", 
                                Member(false, 
                                    Call(
                                        Id("a"), 
                                        [
                                            Member(false, 
                                                Call(
                                                    Id("b"),
                                                    [Member(true, Id("c"), Un("!", Id("d"), true))]
                                                ),
                                                Id("e"))
                                        ]),
                                    Id("f")),
                                Lit("hi", "'hi'")),
                            Lit(2, "2")),
                        Lit(true, "true"))
                },
                { @"(1,2)", Seq(Lit(1, "1"), Lit(2, "2")) },
                {
                    @"(a, a + b > 2)",
                    Seq(
                        Id("a"),
                        Bin(">",
                            Bin("+", Id("a"), Id("b")), 
                            Lit(2, "2")))
                },
                { @"(((1)))", Lit(1, "1") },
                {
                    @"(Object.variable.toLowerCase()).length == 3",
                    Bin("==",
                        Member(false,
                            Call(
                                Member(false,
                                    Member(false,
                                        Id("Object"),
                                        Id("variable")),
                                    Id("toLowerCase"))),
                            Id("length")),
                        Lit(3, "3"))
                },
                {
                    @"(Object.variable.toLowerCase())  .  length == 3",
                    Bin("==",
                        Member(false,
                            Call(
                                Member(false,
                                    Member(false,
                                        Id("Object"),
                                        Id("variable")),
                                    Id("toLowerCase"))),
                            Id("length")),
                        Lit(3, "3"))
                },
                { @"[1] + [2]", Bin("+", Ar(Lit(1, "1")), Ar(Lit(2, "2"))) },
                { @"""a""[0]", Member(true, Lit("a", "\"a\""), Lit(0, "0")) },
                { @"[1](2)", Call(Ar(Lit(1, "1")), [Lit(2, "2")]) },
                { @"""a"".length", Member(false, Lit("a", "\"a\""), Id("length")) },
                { @"a.this", Member(false, Id("a"), Id("this")) },
                { @"a.true", Member(false, Id("a"), Id("true")) }
            };

            Jsep.Initialize();
            var actualNodes = jsonFromJs.ToDictionary(
                (k) => k.Key,
                (v) => JsonConvert.DeserializeObject<SyntaxNode>(v.Value)
            );

            // Check that JS results deserialized properly.
            foreach (var kv in expectedNodes)
            {
                var actual = actualNodes[kv.Key];
                var expected = kv.Value;

                Assert.AreEqual(expected, actual, $"Expression (Validation): \"{kv.Key}\"");
            }

            // Check that parsing the expression produces the same AST in C#.
            foreach (var kv in expectedNodes)
            {
                var actual = Parse(kv.Key);
                var expected = kv.Value;

                Assert.AreEqual(expected, actual, $"Expression (Parse): \"{kv.Key}\"");
            }
        }
    }
}