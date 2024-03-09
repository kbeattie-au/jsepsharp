import { Jsep } from 'jsep';

// Creates a C# dictionary initializer from the expressions supplied and parsed outputs.
function writeCSharpDictInitializer(arr) {
    console.log('{');

    arr.forEach(e => {
        const json = JSON.stringify(Jsep.parse(e));
        const k = e.replace(/"/g, '""');
        const v = json.replace(/"/g, '""');

        // @ indicates a verbatim string literal in C#.
        // Double-quotes are escaped by doubling them ("").
        console.log(`    { @"${k}", @"${v}" },`);
    });

    console.log('}');
}

// Generates output for comparison with C# implementation.
// Used to generate examples in TestJsepJavascriptComparison() in JsepSharp.Tests.csproj.
function generateTestJsepJavascriptComparison() {
    // Extracted (and altered slightly) from jsep/test/jsep.test.js.
    // Alteration: Ternary/conditional (?:) is tested separately in the plugin tests.
    writeCSharpDictInitializer([
        '[1,,3]',
        '[1,,]',
        ' true',
        'false ',
        ' 1.2 ',
        ' .2 ',
        'a',
        'a .b',
        'a.b. c',
        'a [b]',
        'a.b  [ c ] ',
        '$foo[ bar][ baz].other12 [\'lawl\'][12]',
        '$foo     [ 12	] [ baz[z]    ].other12*4 + 1 ',
        '$foo[ bar][ baz]    (a, bb ,   c  )   .other12 [\'lawl\'][12]',
        '(a(b(c[!d]).e).f+\'hi\'==2) === true',
        '(1,2)',
        '(a, a + b > 2)',
        '(((1)))',
        '(Object.variable.toLowerCase()).length == 3',
        '(Object.variable.toLowerCase())  .  length == 3',
        '[1] + [2]',
        '"a"[0]',
        '[1](2)',
        '"a".length',
        'a.this',
        'a.true',
    ]);
}

console.clear();

generateTestJsepJavascriptComparison();