## JsepNet: Tiny JavaScript Expression Parser for .NET

This is a C#, .NET Core conversion of [jsep](https://ericsmekens.github.io/jsep/) project, which is a simple expression parser written in JavaScript. It's designed to parse expressions but not operations. The difference between expressions and operations is akin to the difference between a cell in an Excel spreadsheet vs. a proper JavaScript program.

### Why a C# Version?

I wanted a C# implementation that closely matches the parsing behavior of the JavaScript version. I was working on a different cross-platform project and didn't want inconsistent, incompatible parsing behavior between the C# and JavaScript implementations.

### NuGet Package

A NuGet package is available at TODO.

See package details for the minimum supported .NET Framework and .NET Core versions. They are less strict than the build prerequisites below.

### Build

The solution can be built from within Visual Studio (Community Edition works fine), or through the terminal / command prompt via `msbuild`:

```powershell
test
```

You'll need to make sure it's present in path, or you have a powershell alias, to run the example above. For example, if you're running Windows and using Visual Studio 2022 64-bit Community Edition, the version of `msbuild` you need resides at `"Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin"` in the drive letter you installed it on.

For macOS developers, TODO...

#### Build Prerequisites

* C#: TODO Version.
* .NET Core SDK: TODO Version.

Older versions of both may work, but have not been tested.

### Usage

TODO: Examples

#### Custom Operators

TODO: Explanation of how to add/remove these.

#### Custom Identifiers

TODO: Explanation of how to add/remove these.

### Plugins

The plugins work differently from the JavaScript version, due to language differences.

None are included by default, which is a difference from the JavaScript implementation that includes ternary by default.

#### Included Plugins

| **Name**          | **Description**                                                                            |
|-------------------|--------------------------------------------------------------------------------------------|
| Ternary           | Adds support for ternary `a ? b : c` expressions.                                          |
| Arrow             | Adds support for arrow functions: `v => !!v`                                               |
| Assignment        | Adds support for assignment and update expressions: `a = 2`, `a++`                         |
| AsyncAwait        | Adds support for `async` and `await` expressions.                                          |
| Comment           | Adds support for ignoring comments: `a /* ignore this */ > 1 // ignore this too`           |
| New               | Adds support for `new` keyword: `new Date()`                                               |
| Numbers           | Adds support for hex, octal, binary literals, and `_` literal separators.                  |
| Object            | Adds support for object expressions: `{ a: 1, b: { c }}`                                   |
| Regex             | Adds support for regular expression literals: `/[a-z]{2}/ig`                               |
| Spread            | Adds support for the spread operator, `fn(...[1, ...a])`. Works with `object` plugin, too. |
| TemplateLiteral   | Adds support for template and tagged template literals: `` `hi ${name}` ``                 |

#### How to Add Plugins

TODO

#### List Plugins

TODO

#### Writing Your Own Plugin

TODO

The Comment and Ternary plugins are both relatively simple, as far as built-in plugins go, so they may serve as some good additional examples.

##### Hooks (Events)

The hooks system uses C# events, which differs from the JavaScript implementation. This was done since the C# language already has the necessary feature built-in.

TODO: Description

##### Hook Types

TODO: Update these names.

* `before-all`: called just before starting all expression parsing.
* `after-all`: called after parsing all. Read/Write `arg.node` as required.
* `gobble-expression`: called just before attempting to parse an expression. Set `arg.node` as required.
* `after-expression`: called just after parsing an expression. Read/Write `arg.node` as required.
* `gobble-token`: called just before attempting to parse a token. Set `arg.node` as required.
* `after-token`: called just after parsing a token. Read/Write `arg.node` as required.
* `gobble-spaces`: called when gobbling whitespace.

### License

JsepNet is a derivative work under the MIT license. See LICENSE file.

### Thanks

To all the contributors of the original jsep project.
