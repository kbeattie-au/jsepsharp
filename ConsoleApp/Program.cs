using JsepNet.Plugins;
using JsepNet.SyntaxTree;
using Newtonsoft.Json;

namespace JsepNet.ConsoleApp
{
    internal class Program
    {
        static void Main()
        {
            Jsep.Initialize();

            const string JSON = @"{""type"":""BinaryExpression"",""operator"":""+"",""left"":{""type"":""Identifier"",""name"":""a""},""right"":{""type"":""Literal"",""value"":3,""raw"":""3""}}";
            var deserial = JsonConvert.DeserializeObject<SyntaxNode>(JSON);
            var reJson = JsonConvert.SerializeObject(deserial, Formatting.Indented);
            Console.WriteLine(reJson);
            Console.ReadKey(true);
        }

        static void MainOld()
        {
            string[] expressions = [
                "intentional(error",
                "543error",
                "abs(-4)",
                "2 + 2",
                "'test'",
                "true",
                "[1,2,3,4]",
                "(1 + 2) * 3",
                "a + \"name\"",
                "this.property",
                "window.location",
                "1,2",
                "[1,2] + [3]",
                "method(a, b)",
                "window.alert('hello')",
                ".023",
                "_.max([1,2,3,4])",
                "3 + (33 == 1 + 2 * 3 > 3)",
                // Requires plugins:
                "a == 4 ? 4 + 2 : 3",
                "2 + /* This is a Comment */ 3",
                "2 + 4 // Also a comment",
                "new Basic()",
                "new Basic.Object(1, 2)",
                "new arr[0](1)",
                "{ \"Hello\": 123 }",
                "{ Goodbye: true, Entries: [] }",
                "[...a]",
                "{ ...a, b: true }",
                "a => a + 4",
                "() => 3",
                "(x, y) => x + y",
                "async () => await load()",
                "`tes\\nt`",
                "`test${a}`",
                "`x${a}y${b}z`",
                "`${a}${b}`",
                "`${(1+2)}`",
                "tagFunction`hello${world}`",
                "a = 4",
                "a = 1 + 3",
                "a += b(4)",
                "++a",
                "a++",
                "/[a-zA-Z]?/i",
                "/^\\d{3}(.*)?EOR$/m",
                "0x12FF",
                "0X12FE",
                "0B10010001",
                "0b10010000",
                "0o777",
                "0O776",
                "0775"
            ];

            int numExpressions = expressions.Length;
            int i = 0;
            while (i < numExpressions)
            {
                ApplyNavigation(
                    ParseAndDisplay(expressions[i], i + 1, numExpressions),
                    numExpressions,
                    ref i);
            }
        }

        static int ParseAndDisplay(string expression, int num, int total)
        {
            // TIP: If using Windows Terminal, use CTRL+SHIFT+HOME to scroll to the top of the screen.
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.Clear();
            Console.WriteLine("\x1b[3J"); // Clear() doesn't always clear everything when scrolling happens (Windows Terminal).
            Console.Clear();

            Console.WriteLine($"Expression ({num} of {total}):");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(expression);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            try
            {
                var node = Jsep.Parse(expression, BuiltIns.All);
                var json = JsonConvert.SerializeObject(node, Formatting.Indented);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(json);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Parsing Failed!");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            return ReadNavigationFromKey();
        }

        static int ReadNavigationFromKey()
        {
            var ki = Console.ReadKey(true);
            while (ki.Key != ConsoleKey.Enter &&
                ki.Key != ConsoleKey.Spacebar &&
                ki.Key != ConsoleKey.Q &&
                ki.Key != ConsoleKey.Escape &&
                ki.Key != ConsoleKey.F10 &&
                ki.Key != ConsoleKey.UpArrow &&
                ki.Key != ConsoleKey.LeftArrow &&
                ki.Key != ConsoleKey.PageUp &&
                ki.Key != ConsoleKey.DownArrow &&
                ki.Key != ConsoleKey.RightArrow &&
                ki.Key != ConsoleKey.PageDown &&
                ki.Key != ConsoleKey.Home &&
                ki.Key != ConsoleKey.End)
            {
                ki = Console.ReadKey(true);
            }

            if (ki.Key == ConsoleKey.Q || ki.Key == ConsoleKey.Escape || ki.Key == ConsoleKey.F10) return -3;
            if (ki.Key == ConsoleKey.Home) return -2;
            if (ki.Key == ConsoleKey.UpArrow || ki.Key == ConsoleKey.LeftArrow || ki.Key == ConsoleKey.PageUp) return -1;
            if (ki.Key == ConsoleKey.Enter || ki.Key == ConsoleKey.Spacebar) return 2;
            if (ki.Key == ConsoleKey.End) return 3;

            return 1;
        }

        static void ApplyNavigation(int navigationResult, int totalPages, ref int page)
        {
            switch (navigationResult)
            {
                case -3: // Quit
                    page = totalPages;
                    break;
                case -2: // Go to Beginning
                    page = 0;
                    break;
                case -1: // Back
                    if (page > 0) --page;
                    break;
                case 1: // Forward
                    if (page + 1 < totalPages) ++page;
                    break;
                case 2: // Forward And Quit If End
                    ++page;
                    break;
                case 3: // Go to End
                    page = totalPages - 1;
                    break;
            }
        }
    }
}
