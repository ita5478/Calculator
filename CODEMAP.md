# Code Map

This document maps the codebase: the layers, every meaningful file, and the exact path a
single expression takes from raw string to numeric result. For a higher-level overview and
run instructions, see [README.md](README.md).

## 1. The big picture

```
            ┌──────────────────────── Front-ends ────────────────────────┐
            │  ConsoleApp1      Calculator.WebApi    Calculator.BlazorUI  │
            └───────────────────────────┬────────────────────────────────┘
                                         │ ICalculatorUi.Solve(string)
                                         ▼
                              ┌──────────────────────┐
                              │   Calculator.UI      │  composition root + I/O-facing pipeline
                              │  (Bootstrapper)      │
                              └──────────┬───────────┘
                                         │
                       ┌─────────────────┴─────────────────┐
                       ▼                                    ▼
            ┌──────────────────┐                 ┌────────────────────┐
            │  Calculator.BL   │                 │  Calculator.Common │  generic contracts
            │ tokens, shunting │                 │ IParser/IConverter │
            │ yard, conversion │                 │ ITransformer/...   │
            └────────┬─────────┘                 └────────────────────┘
                     ▼
            ┌──────────────────┐
            │ Calculator.Core  │  the evaluation domain (ICalculatable tree + operations)
            └──────────────────┘
```

Dependencies point inward only. `Core` and `Common` depend on nothing in the solution.

## 2. The request pipeline (the important part)

Everything starts at [Calculator.UI/Implementations/CalculatorUI.cs](Calculator.UI/Implementations/CalculatorUI.cs).
`Solve("2 + sqrt(16)")` runs three stages:

```
string ──Parse──▶ IEnumerable<Token> ──Convert──▶ ICalculatable ──Calculate()──▶ float
         (1)                          (2)                          (3)
```

### Stage 1 — Parse: string → tokens
`ExpressionParser.Parse` ([Calculator.UI/Implementations/ExpressionParser.cs](Calculator.UI/Implementations/ExpressionParser.cs))
- Splits the raw string with a regex (`\b(sqrt|abs)\b|([*^+/\-)(])|([0-9.]+|.)`, defined in the `Bootstrapper`).
- Hands each raw fragment to `Tokenizer.Tokenize` ([Calculator.UI/Implementations/Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs)),
  which classifies it into a `TokenType` (Number / BinaryOperation / UnaryOperation / OpeningBracket / ClosingBracket)
  by testing it against a set of validation functions. Numbers are validated by
  `NumbersValidator` ([Calculator.UI/Implementations/NumbersValidator.cs](Calculator.UI/Implementations/NumbersValidator.cs)).
- Unknown fragments throw `InvalidTokenException`; an empty result throws `InvalidExpressionException`.
- Output: a list of `Token` (value + `TokenType`) — see [Calculator.BL/Token.cs](Calculator.BL/Token.cs).

### Stage 2 — Convert: tokens → an evaluable tree
`ExpressionToCalculatableConverter.Convert` ([Calculator.BL/Implementations/ExpressionToCalculatableConverter.cs](Calculator.BL/Implementations/ExpressionToCalculatableConverter.cs))
does two sub-steps:

1. **Reorder (infix → postfix)** via `ShuntingYardTransformer.Transform`
   ([Calculator.BL/Implementations/ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs)).
   This is the classic Shunting-Yard algorithm using operator precedence and bracket pairs.
   It is where bracket-matching errors are raised (`OpeningBracketMissingException`,
   `ClosingBracketMissingException`, `MismatchedBracketPairException`).
2. **Build the tree** by walking the postfix tokens and pushing/popping an operand stack.
   Dispatch is done by `TokenActionHandler`
   ([Calculator.BL/Implementations/TokenActionHandlers/TokenActionHandler.cs](Calculator.BL/Implementations/TokenActionHandlers/TokenActionHandler.cs)),
   which routes each token by `TokenType` to:
   - `NumberTokenHandler` → pushes a `CalculatableNumber`.
   - `BinaryOperationTokenHandler` → pops two operands, asks the matching factory to build a node, pushes it.
   - `UnaryOperationTokenHandler` → pops one operand, builds a node, pushes it.

   Stack-underflow ⇒ `MissingOperandException`; leftover operands ⇒ `MissingOperatorException`.
   Output: the single root `ICalculatable`.

### Stage 3 — Calculate: tree → number
`root.Calculate()` recurses through the `ICalculatable` tree
([Calculator.Core/Abstractions/ICalculatable.cs](Calculator.Core/Abstractions/ICalculatable.cs)).
Leaves return their value; operation nodes combine their children's results. Domain errors
surface here (e.g. `DivideByZeroException` in `Division`, negative-input guard in `SquareRoot`).

## 3. Project-by-project file index

### Calculator.Common — generic pipeline contracts
Domain-agnostic interfaces that let each pipeline stage depend on an abstraction.
- [Abstractions/IParser.cs](Calculator.Common/Abstractions/IParser.cs) — `Parse(string) : T`
- [Abstractions/ITransformer.cs](Calculator.Common/Abstractions/ITransformer.cs) — `Transform(T) : T`
- [Abstractions/IConverter.cs](Calculator.Common/Abstractions/IConverter.cs) — `Convert(TFrom) : TTo`
- [Abstractions/IValidator.cs](Calculator.Common/Abstractions/IValidator.cs) — `Validate(T) : bool`

### Calculator.Core — the evaluation domain
The composite tree and the operations, with a factory per operation.
- [Abstractions/ICalculatable.cs](Calculator.Core/Abstractions/ICalculatable.cs) — tree node contract (`Calculate()`).
- [Abstractions/BinaryOperationBase.cs](Calculator.Core/Abstractions/BinaryOperationBase.cs), [Abstractions/UnaryOperationBase.cs](Calculator.Core/Abstractions/UnaryOperationBase.cs) — base classes holding operands.
- [Abstractions/IOperationPrecedence.cs](Calculator.Core/Abstractions/IOperationPrecedence.cs) — `Precedence` (used by Shunting-Yard).
- [Abstractions/IBinaryOperationFactory.cs](Calculator.Core/Abstractions/IBinaryOperationFactory.cs), [Abstractions/IUnaryOperationFactory.cs](Calculator.Core/Abstractions/IUnaryOperationFactory.cs) — factories (each also carries precedence).
- [Implementations/CalculatableNumber.cs](Calculator.Core/Implementations/CalculatableNumber.cs) — leaf node wrapping a `float`.
- `Implementations/BinaryOperations/` — `Addition`, `Multiplication`, `Division`, `Power`.
- `Implementations/UnaryOperations/` — `Minus`, `SquareRoot`, `Absolute`.
- `Implementations/BinaryOperationFactories/` & `Implementations/UnaryOperationFactories/` — one factory per operation above, carrying its precedence.

### Calculator.BL — business logic
- [Token.cs](Calculator.BL/Token.cs) — value + `TokenType`.
- [Enums/TokenType.cs](Calculator.BL/Enums/TokenType.cs) — Number / BinaryOperation / UnaryOperation / Opening- / ClosingBracket.
- [BracketPair.cs](Calculator.BL/BracketPair.cs) — an opening/closing bracket pair + membership tests.
- [Implementations/ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs) — infix→postfix.
- [Implementations/ExpressionToCalculatableConverter.cs](Calculator.BL/Implementations/ExpressionToCalculatableConverter.cs) — postfix→tree.
- [Abstractions/ITokenActionHandler.cs](Calculator.BL/Abstractions/ITokenActionHandler.cs) + `Implementations/TokenActionHandlers/` — per-`TokenType` stack handlers and their dispatcher.
- `Exceptions/` — domain exceptions: `MissingOperandException`, `MissingOperatorException`, `OpeningBracketMissingException`, `ClosingBracketMissingException`, `MismatchedBracketPairException`.

### Calculator.UI — composition + I/O-facing pipeline
- [Abstractions/ICalculatorUi.cs](Calculator.UI/Abstractions/ICalculatorUI.cs) — the facade front-ends depend on (`Solve(string) : float`).
- [Abstractions/ITokenizer.cs](Calculator.UI/Abstractions/ITokenizer.cs) — `Tokenize(string) : Token`.
- [Implementations/CalculatorUI.cs](Calculator.UI/Implementations/CalculatorUI.cs) — orchestrates the 3 stages.
- [Implementations/ExpressionParser.cs](Calculator.UI/Implementations/ExpressionParser.cs), [Implementations/Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs), [Implementations/NumbersValidator.cs](Calculator.UI/Implementations/NumbersValidator.cs).
- [Exceptions/InvalidTokenException.cs](Calculator.UI/Exceptions/InvalidTokenException.cs).
- **[Bootstrapper.cs](Calculator.UI/Bootstrapper.cs) — the composition root.** Defines the splitting
  regex, the operator → factory dictionaries, the bracket pairs, the precedence map, the token
  handlers, and wires up the entire object graph into a `CalculatorUi`. **This is the one place
  you edit to add an operator or bracket type.**

### Front-ends (all consume `ICalculatorUi`)
- [ConsoleApp1/Program.cs](ConsoleApp1/Program.cs) — REPL loop reading lines from the console.
- [Calculator.WebApi/Controllers/CalculatorController.cs](Calculator.WebApi/Controllers/CalculatorController.cs) — `POST /Calculator`; registers the calculator as a singleton in [Program.cs](Calculator.WebApi/Program.cs).
- [Calculator.BlazorUI/](Calculator.BlazorUI/) — Blazor (server host + WASM client) with a button-grid UI in
  [Calculator.BlazorUI.Client/Pages/Calculator.razor](Calculator.BlazorUI/Calculator.BlazorUI.Client/Pages/Calculator.razor);
  the calculator is injected via DI from the server [Program.cs](Calculator.BlazorUI/Calculator.BlazorUI/Program.cs).

### Calculator.BL.Tests
NUnit test project targeting the BL/Core logic. (Only build artifacts are present in the
working tree; the test sources live with the `feature/unit-testing` work.)

## 4. Extension recipes

**Add a binary operator** (say modulo `%`):
1. `Modulo : BinaryOperationBase` in `Calculator.Core/Implementations/BinaryOperations/`.
2. `ModuloFactory : IBinaryOperationFactory` with a `Precedence` in `.../BinaryOperationFactories/`.
3. One entry in the `binaryOperations` dictionary in [Bootstrapper.cs](Calculator.UI/Bootstrapper.cs)
   and, if the symbol isn't already covered, the splitting regex.

**Add a unary function** (e.g. `sin`): same pattern using `UnaryOperationBase` /
`IUnaryOperationFactory` and the `unaryOperations` dictionary (add the keyword to the regex's
`\b(sqrt|abs)\b` group).

**Add a bracket type:** add a `BracketPair` to the `bracketPairs` list in the `Bootstrapper`
(and the characters to the regex).

**Add a front-end:** call `Bootstrapper.Initialize()` (or DI-register the resulting
`ICalculatorUi`) and call `Solve`.
