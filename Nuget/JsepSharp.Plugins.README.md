# JsepSharp Plugins

These are the standard optional plugins for the JsepSharp parser. They add support for functionality not available in the core implementation.

## Usage

```csharp
// Create instance to use with plugins:
var j = new Jsep("a ? b : c");
j.RegisterPlugin(typeof(TernaryPlugin));
j.Parse();

// Static parse method with all built-in plugins:
Jsep.Parse("f = (a) => ({ b: 4, c: (d > 4 ? 4 : d), e: /^[a-z]{5}$/ })", BuiltIns.All);

// Static parse method With specific plugins:
Jsep.Parse("a => b ? c : d", new[] { typeof(TernaryPlugin), typeof(ArrowPlugin) });
```

*Note: Plugin inclusion order matters. Ternary should generally be included before other plugins, if it is being used.*

## Plugins

The plugins work differently from the JavaScript version, due to language differences.

None are included by default, which is a *difference* from the JavaScript implementation that includes ternary by default.

### Included Plugins

| **Name**            | **Description**                                                                              |
| ------------------- | -------------------------------------------------------------------------------------------- |
| **Ternary**         | Adds support for ternary expressions: `a ? b : c`                                            |
| **Arrow**           | Adds support for arrow functions: `v => !!v`, `(x, y) => x + y`                              |
| **Assignment**      | Adds support for assignment and update expressions: `a = 2`, `a++`                           |
| **AsyncAwait**      | Adds support for `async` and `await` keywords.                                               |
| **Comment**         | Adds support for ignoring comments: `a /* ignore this */ > 1 // ignore this too`             |
| **New**             | Adds support for `new` keyword: `new Date()`                                                 |
| **Numbers**         | Adds support for hex, octal, binary literals, and `_` literal separators.                    |
| **Object**          | Adds support for object expressions: `{ a: 1, b: { c }}`                                     |
| **Regex**           | Adds support for regular expression literals: `/[a-z]{2}/ig`                                 |
| **Spread**          | Adds support for the spread operator, `fn(...[1, ...a])`. Works with **Object** plugin, too. |
| **TemplateLiteral** | Adds support for template and tagged template literals: `` `hi ${name}` ``                   |

### How to Add Plugins

1. Class/Static `Parse` method: Supply the second argument with an enumerable of `System.Type` for each plugin.

2. Instance `RegisterPlugin` method: After creating an instance, invoke `RegisterPlugin` with the `System.Type` of the plugin.

Regardlesss of how you register them, the plugin class must implement abstract class `Jsep.Plugin` . If it does not, an `ArgumentException` will be raised.

### List Plugins

Installed plug-ins can be enumerated from a `Jsep` instance via the `Plugins` property.

### Writing Your Own Plugin

#### Hooks (Events)

The hooks system uses C# events, which differs from the JavaScript implementation. This was done since the C# language already has the necessary feature built-in.

These exist to provide extensibility points that plugins can use to integrate into the parser and add additional functionality to it.

#### Hook Types

* **`BeforeParsing`**: called just before starting all expression parsing. Port of `before-all`
* **`AfterParsing`**: called after parsing all. Read/Write `env.Node` as required. Port of `after-all`.
* **`BeforeExpression`**: called just before attempting to parse an expression. Set `env.Node` as required. Port of `gobble-expression`.
* **`AfterExpression`**: called just after parsing an expression. Read/Write `env.Node` as required. Port of `after-expression`.
* **`BeforeToken`**: called just before attempting to parse a token. Set `env.Node` as required. Port of `gobble-token`.
* **`AfterToken`**: called just after parsing a token. Read/Write `env.Node` as required. Port of `after-token`.
* **`AfterSpaces`**: called when gobbling whitespace. Port of `gobble-spaces`.

#### Examples

* The **Spread** plugin is the smallest of the bulit-ins.
* The **AsyncAwait** plugin demonstrates how to declare a plugin depends upon another plugin.
* The **Arrow** plugin shows how to use `ReplaceNodes()` to alter previously parsed results.
* The **Comment** plugin shows how to instruct the parser to ignore additional sequences.
* The **New** plugin swaps out a node with a new type of node.
* The **Regex** plugin shows how to create a new type of literal to the parser.
* The **Number** plugin shows how to extend the parsing behavior of an existing literal.

### License

The JsepSharp Plugins are a derivative work under the MIT license. See LICENSE file.

### Thanks

To all the contributors of the original [jsep](https://github.com/EricSmekens/jsep/graphs/contributors) project.


