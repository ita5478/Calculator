# Calculator

A string-expression calculator built as a **code design exercise**. The point of the
project is not the arithmetic — it is the architecture: a clean, layered, dependency-inverted
design where new operators, brackets, and front-ends can be added without touching existing
code.

It parses an infix math expression (e.g. `2 + sqrt(16) * (3 ^ 2)`), converts it to postfix
using the [Shunting-Yard algorithm](https://en.wikipedia.org/wiki/Shunting_yard_algorithm),
builds a tree of `ICalculatable` nodes, and evaluates it.

For a detailed, file-by-file walkthrough of how a request flows through the system, see
[CODEMAP.md](CODEMAP.md).

## Supported syntax

| Category | Tokens | Notes |
| --- | --- | --- |
| Numbers | `0-9`, `.` | Parsed as `float` |
| Binary operators | `+`  `*`  `/`  `^` | Subtraction is expressed via the unary minus |
| Unary operators | `-`  `sqrt`  `abs` | `sqrt` rejects negative input; `/` rejects divide-by-zero |
| Brackets | `( )`  `[ ]`  `{ }` | Pairs must match; mismatches/missing brackets throw |

Operator precedence (higher binds tighter): `+` = 0, `*` = `/` = 1, `^` = 2, unary `-` = 1,
`sqrt` = `abs` = 2.

## Solution layout

The solution is split into three **library** layers plus three interchangeable **front-ends**.
Dependencies only ever point inward (front-ends → UI → BL → Core/Common).

| Project | Role | Depends on |
| --- | --- | --- |
| `Calculator.Common` | Generic, domain-agnostic contracts (`IParser`, `IConverter`, `ITransformer`, `IValidator`) | — |
| `Calculator.Core` | The evaluation domain: `ICalculatable` tree, operations and their factories | — |
| `Calculator.BL` | Business logic: tokens, Shunting-Yard transform, token→`ICalculatable` conversion | Common, Core |
| `Calculator.UI` | Application/composition layer: tokenizer, parser, `CalculatorUi` facade, `Bootstrapper` | Common, Core, BL |
| `Calculator.ConsoleApp` | REPL front-end | UI |
| `Calculator.WebApi` | REST front-end (`POST /Calculator`) | UI |
| `Calculator.BlazorUI` | Blazor (server + WASM) button-grid front-end | UI |
| `Calculator.BL.Tests` | NUnit tests for the BL/Core logic | BL, Core |

> Note: the library layers target **.NET 6**; the Blazor front-ends target **.NET 8**.

## Design ideas worth noting

- **Dependency inversion via `Calculator.Common`.** The pipeline stages are expressed as
  generic interfaces (`IParser<T>`, `ITransformer<T>`, `IConverter<TFrom,TTo>`,
  `IValidator<T>`), so each stage depends on an abstraction rather than a concrete class.
- **Composite pattern for evaluation.** An expression becomes a tree of `ICalculatable`
  nodes (`CalculatableNumber`, `Addition`, `SquareRoot`, …). Calling `Calculate()` on the
  root recurses through the tree — no separate evaluator switch statement.
- **Factory + registry for operators.** Each operator has a factory carrying its precedence.
  Adding an operator = add an operation class + a factory + one dictionary entry in the
  `Bootstrapper`. Nothing else changes.
- **Strategy/dispatch for tokens.** `TokenActionHandler` dispatches each token to a handler
  keyed by `TokenType`, keeping the conversion loop free of type-checking branches.
- **Single composition root.** `Bootstrapper.Initialize()` wires the whole object graph; the
  three front-ends share it, differing only in how they collect input and present output.

## Running

Pick a front-end:

```bash
# Console REPL
dotnet run --project ConsoleApp1

# REST API (POST a JSON string body to /Calculator)
dotnet run --project Calculator.WebApi

# Blazor web UI
dotnet run --project Calculator.BlazorUI/Calculator.BlazorUI
```

Run the tests:

```bash
dotnet test Calculator.BL.Tests
```
