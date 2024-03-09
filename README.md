## JsepSharp: Tiny JavaScript Expression Parser Implementation for .NET

This is a C# .NET Core conversion of the [jsep](https://ericsmekens.github.io/jsep/) project, which is a simple expression parser written in JavaScript. 

This library is designed to parse expressions but not operations. The difference between expressions and operations is akin to the difference between a cell in an Excel spreadsheet vs. a proper JavaScript program. In other words, there is no support for constructs like `if`, `while`, `try`, `function`, `class`, etc.

### Why a C# Version?

I wanted a C# implementation that closely matches the parsing behavior of the JavaScript version. I was working on a different cross-platform project and didn't want inconsistent, incompatible parsing behavior between the C# and JavaScript implementations.

### How Do I Run Parsed Expressions?

This library just just a parser. It does not attempt to execute or compile the parsed AST (abstract syntax tree).

A minimal example interpreter may be written in the future (likely in a separate repository), if there is a demand for one.

### NuGet Package

**A NuGet package does not yet exist.**

When it does, see the package details for the minimum supported .NET Framework and .NET Core versions. They are expected to be less strict than the build prerequisites below.

### Build

The solution can be built from within Visual Studio (Community Edition works fine), or through the terminal / command prompt via `dotnet`:

```powershell
dotnet build
```

You'll need to make sure you have the .NET Core 8.x (or newer) SDK installed, or the command will not work.

#### Build Prerequisites

* C#: 12.x.
* .NET Core SDK: 8.x

Older versions of both may work with (or without) some code adjustments, but have not been tested.

### Testing

Automated tests can be run from within the Test Explorer in Visual Studio, or via a terminal / command prompt via `dotnet`:

```powershell
dotnet test
```

### Usage

```csharp
// Create instance to use without plugins:
var j = new Jsep("a + 2");
j.Parse();

// Use without plugins (static):
Jsep.Parse("a + 2");

// With all built-in plugins (static):
Jsep.Parse("f = (a) => ({ b: 4, c: (d > 4 ? 4 : d), e: /^[a-z]{5}$/ })", BuiltIns.All);

// With specific plugins (static):
Jsep.Parse("a => b ? c : d", new[] { typeof(TernaryPlugin), typeof(ArrowPlugin) });
```

*Note: Plugin inclusion order matters. Ternary should generally be included before other plugins, if it is being used.*

#### Custom Operators

Methods are available to add, remove, and reset custom unary and binary operators.

```csharp
// Add and remove a left-associative spaceship / comparison operator with same precedence (7) as other comparisons (<, >, <=, >=).
Jsep.AddBinaryOp("<=>", 7, false);
Jsep.RemoveBinaryOp("<=>");

// Add and remove a bit inversion operator.
Jsep.AddUnaryOp("~");
Jsep.RemoveUnaryOp("~");

// Reset all operators to defaults / built-ins.
Jsep.ResetBinaryOps();
Jsep.ResetUnaryOps();
```

#### Custom Identifiers

Methods are available to add, remove, and reset custom identifier characters.

```csharp
// Add/remove ability for identifiers to contain the @ symbol.
Jsep.AddIdentifier('@');
Jsep.RemoveIdentifier('@');

// Reset back to defaults.
Jsep.ResetIdentifiers();
```

#### Custom Literals

Methods are available to add, remove, and reset custom literal transformations. These are used to convert keywords into raw literals (e.g. "true" becoming a boolean `true`):

```csharp
// Add/remove nil alias for null.
Jsep.AddLiteral("nil", null);
Jsep.RemoveLiteral("nil");

// Reset back to defaults.
Jsep.ResetLiterals();
```

### Plugins

The plugins work differently from the JavaScript version, due to language differences.

None are included by default, which is a *difference* from the JavaScript implementation that includes ternary by default.

#### Included Plugins

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

#### How to Add Plugins

1. Class/Static `Parse` method: Supply the second argument with an enumerable of `System.Type` for each plugin.

2. Instance `RegisterPlugin` method: After creating an instance, invoke `RegisterPlugin` with the `System.Type` of the plugin.

Regardlesss of how you register them, the plugin class must implement abstract class `Jsep.Plugin` . If it does not, an `ArgumentException` will be raised.

#### List Plugins

Installed plug-ins can be enumerated from a `Jsep` instance via the `Plugins` property.

#### Writing Your Own Plugin

##### Hooks (Events)

The hooks system uses C# events, which differs from the JavaScript implementation. This was done since the C# language already has the necessary feature built-in.

These exist to provide extensibility points that plugins can use to integrate into the parser and add additional functionality to it.

##### Hook Types

* **`BeforeParsing`**: called just before starting all expression parsing. Port of `before-all`
* **`AfterParsing`**: called after parsing all. Read/Write `env.Node` as required. Port of `after-all`.
* **`BeforeExpression`**: called just before attempting to parse an expression. Set `env.Node` as required. Port of `gobble-expression`.
* **`AfterExpression`**: called just after parsing an expression. Read/Write `env.Node` as required. Port of `after-expression`.
* **`BeforeToken`**: called just before attempting to parse a token. Set `env.Node` as required. Port of `gobble-token`.
* **`AfterToken`**: called just after parsing a token. Read/Write `env.Node` as required. Port of `after-token`.
* **`AfterSpaces`**: called when gobbling whitespace. Port of `gobble-spaces`.

##### Examples

* The **Spread** plugin is the smallest of the bulit-ins.
* The **AsyncAwait** plugin demonstrates how to declare a plugin depends upon another plugin.
* The **Arrow** plugin shows how to use `ReplaceNodes()` to alter previously parsed results.
* The **Comment** plugin shows how to instruct the parser to ignore additional sequences.
* The **New** plugin swaps out a node with a new type of node.
* The **Regex** plugin shows how to create a new type of literal to the parser.
* The **Number** plugin shows how to extend the parsing behavior of an existing literal.

### Compatibility with JavaScript Version

#### Goals

* Similar parsing behavior and JSON output.
* Same extensibility points (adding/removing operators, literals, and custom identifier characters).
* Support for same hooks and all available built-in plugins in Jsep.
* Avoid deviating too far from the original JavaScript design, as it makes changes and fixes more difficult to convert over to C#.
* Ability to parse dumped JSON results from the JavaScript implementation.

#### Non-Goals

* Exact same methods and variable names. C# [style conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions#style-guidelines) follow different rules from JavaScript, in terms of capitalization, brace placement, etc.
* Exact same JSON output. Some optional boolean properties may not be emitted when they're false.
* Exact same implementation. This is not possible due to language differences, such as those found with the type system. Though in many places the implementation is pretty close, there are other places where code had to be implemented differently.
* Esprima comparisons in tests. It is a recommended to have some tests compare outputs from parsing against the JavaScript implementation, but comparing additionally against Esprima like the JavaScript implementation does in some tests is not necessary.

#### Known Differences

* The `TemplateElement`  node does not place `raw` and `cooked`  under a nested `value` property. This is likely to change in the future to match the JavaScript behavior.

### License

JsepSharp is a derivative work under the MIT license. See LICENSE file.

### Thanks

To all the [contributors](https://github.com/EricSmekens/jsep/graphs/contributors) of the original jsep project.
