# JsepSharp

This library is designed to parse expressions but not operations. The difference between expressions and operations is akin to the difference between a cell in an Excel spreadsheet vs. a proper JavaScript program. In other words, there is no support for constructs like `if`, `while`, `try`, `function`, `class`, etc.

This is a C# .NET Core conversion of the [jsep](https://ericsmekens.github.io/jsep/) project, which is a simple expression parser written in JavaScript. 

## Can I Run Parsed Expressions?

This library just just a parser. It does not attempt to execute or compile the parsed AST (abstract syntax tree).

## Usage

```csharp
// Create instance to use
var j = new Jsep("a + 2");
j.Parse();

// Use static parse
Jsep.Parse("a + 2");
```

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

### License

JsepSharp is a derivative work under the MIT license. See LICENSE file.

### Thanks

To all the contributors of the original [jsep](https://github.com/EricSmekens/jsep) project.
