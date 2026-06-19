# Code Map

This document maps the codebase: the layers, every meaningful file, and the exact path a
single expression takes from raw string to numeric result. For a higher-level overview and
run instructions, see [README.md](README.md).

## 1. The big picture

```
            ┌──────────────────────────── Front-ends ────────────────────────────┐
            │  Calculator.ConsoleApp   Calculator.WebApi    Calculator.BlazorUI   │
            └─────────────────────────────────┬───────────────────────────────────┘
                                               │ ICalculatorUi.Solve(string)
                                               ▼
                                    ┌──────────────────────┐
                                    │   Calculator.UI      │  composition root + I/O-facing pipeline
                                    │  (Bootstrapper)      │
                                    └──────────┬───────────┘
                                               │
                  ┌────────────────────────────┼────────────────────────────┐
                  ▼                             ▼                            ▼
        ┌──────────────────┐         ┌────────────────────┐      ┌────────────────────┐
        │  Calculator.BL   │         │  Calculator.Common │      │ Calculator.Kernel  │
        │ shunting yard,   │         │ IParser/IConverter │      │ Token / TokenType  │
        │ conversion       │         │ ITransformer/...   │      │ BracketPair        │
        └────────┬─────────┘         └────────────────────┘      └────────────────────┘
                 ▼
        ┌──────────────────┐
        │ Calculator.Core  │  the evaluation domain (ICalculatable tree + operations)
        └──────────────────┘
```

Dependencies point inward only. `Core`, `Common` and `Kernel` depend on nothing in the
solution. `BL` → (Common, Core, Kernel); `UI` → (BL, Common, Core, Kernel); front-ends → UI.
All projects target **net8.0**, pinned centrally in [Directory.Build.props](Directory.Build.props).

`Calculator.Kernel` holds the shared token vocabulary (`Token`, `TokenType`, `BracketPair`)
that both `UI` (which produces tokens) and `BL` (which consumes them) depend on, so neither
front-end-facing layer has to reach into the other for it.

## 2. The request pipeline (the important part)

Everything starts at [Calculator.UI/Implementations/CalculatorUi.cs](Calculator.UI/Implementations/CalculatorUi.cs).
`Solve("2 + sqrt(16)")` runs three stages:

```
string ──Parse──▶ IEnumerable<Token> ──Convert──▶ ICalculatable ──Calculate()──▶ float
         (1)                          (2)                          (3)
```

`CalculatorUi` depends only on the `IParser` and `IConverter` abstractions (not the concrete
parser/converter), so the orchestration is fully mockable.

### Stage 1 — Parse: string → tokens
`ExpressionParser.Parse` ([Calculator.UI/Implementations/ExpressionParser.cs](Calculator.UI/Implementations/ExpressionParser.cs))
- Splits the raw string with a regex (`\b(sqrt|abs)\b|([*^+/\-)(])|([0-9.]+|.)`, defined in the `Bootstrapper`).
- Hands each raw fragment **plus the previous token** to `Tokenizer.Tokenize`
  ([Calculator.UI/Implementations/Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs)),
  which classifies it into a `TokenType` (Number / BinaryOperation / UnaryOperation / OpeningBracket / ClosingBracket)
  by testing it against a set of validation functions. The previous token is what lets the
  tokenizer disambiguate **unary vs. binary `-`** (unary at the start of an expression, after
  another operator, or after an opening bracket; binary otherwise). Numbers are validated by
  `NumbersValidator` ([Calculator.UI/Implementations/NumbersValidator.cs](Calculator.UI/Implementations/NumbersValidator.cs)),
  which parses with `CultureInfo.InvariantCulture` and rejects `NaN`/`Infinity`.
- Unknown fragments throw `InvalidTokenException`; an empty result throws `InvalidExpressionException`.
- Output: a list of `Token` (value + `TokenType`) — see [Calculator.Kernel/Token.cs](Calculator.Kernel/Token.cs).

### Stage 2 — Convert: tokens → an evaluable tree
`ExpressionToCalculatableConverter.Convert` ([Calculator.BL/Implementations/ExpressionToCalculatableConverter.cs](Calculator.BL/Implementations/ExpressionToCalculatableConverter.cs))
does two sub-steps:

1. **Reorder (infix → postfix)** via `ShuntingYardTransformer.Transform`
   ([Calculator.BL/Implementations/ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs)).
   This is the classic Shunting-Yard algorithm using operator **precedence and associativity**
   (right-associative operators such as `^` pop on `>` rather than `>=`) and bracket pairs.
   It reads its input without mutating it and returns a fresh collection. It is where
   bracket-matching errors are raised (`OpeningBracketMissingException`,
   `ClosingBracketMissingException`, `MismatchedBracketPairException`).
2. **Build the tree** by walking the postfix tokens and pushing/popping an operand stack.
   Dispatch is done by `TokenActionHandler`
   ([Calculator.BL/Implementations/TokenActionHandlers/TokenActionHandler.cs](Calculator.BL/Implementations/TokenActionHandlers/TokenActionHandler.cs)),
   which routes each token by `TokenType` to:
   - `NumberTokenHandler` → pushes a `CalculatableNumber` (invariant-culture parse).
   - `BinaryOperationTokenHandler` → pops two operands, asks the matching factory to build a node, pushes it.
   - `UnaryOperationTokenHandler` → pops one operand, builds a node, pushes it.

   Stack-underflow ⇒ `MissingOperandException`; leftover operands ⇒ `MissingOperatorException`.
   The terminal pop is guarded, so an empty result (e.g. `()`) raises a domain
   `MissingOperandException` rather than leaking a BCL `InvalidOperationException`.
   Output: the single root `ICalculatable`.

### Stage 3 — Calculate: tree → number
`root.Calculate()` recurses through the `ICalculatable` tree
([Calculator.Core/Abstractions/ICalculatable.cs](Calculator.Core/Abstractions/ICalculatable.cs)).
Leaves return their value; operation nodes combine their children's results. Domain errors
surface here (e.g. `DivideByZeroException` in `Division`, and `NegativeSquareRootException` from
`SquareRoot` when the operand is negative).

## 3. Project-by-project file index

### Calculator.Kernel — shared token vocabulary
The small set of types that both the UI (token producer) and BL (token consumer) share.
- [Token.cs](Calculator.Kernel/Token.cs) — value + `TokenType`.
- [Enums/TokenType.cs](Calculator.Kernel/Enums/TokenType.cs) — Number / BinaryOperation / UnaryOperation / Opening- / ClosingBracket.
- [BracketPair.cs](Calculator.Kernel/BracketPair.cs) — an opening/closing bracket pair + membership tests.

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
- [Abstractions/IOperationPrecedence.cs](Calculator.Core/Abstractions/IOperationPrecedence.cs) — `Precedence` + `Associativity` (consumed by Shunting-Yard).
- [Abstractions/OperationAssociativity.cs](Calculator.Core/Abstractions/OperationAssociativity.cs) — `Left` / `Right`.
- [Implementations/OperationPrecedence.cs](Calculator.Core/Implementations/OperationPrecedence.cs) — the precedence/associativity value type registered per operator.
- [Abstractions/IBinaryOperationFactory.cs](Calculator.Core/Abstractions/IBinaryOperationFactory.cs), [Abstractions/IUnaryOperationFactory.cs](Calculator.Core/Abstractions/IUnaryOperationFactory.cs) — factories that expose only `Create(...)` (precedence is registered separately, not carried by the factory).
- [Implementations/CalculatableNumber.cs](Calculator.Core/Implementations/CalculatableNumber.cs) — leaf node wrapping a `float`.
- `Implementations/BinaryOperations/` — `Addition`, `Subtraction`, `Multiplication`, `Division`, `Power`.
- `Implementations/UnaryOperations/` — `Minus`, `SquareRoot`, `Absolute`.
- `Implementations/BinaryOperationFactories/` & `Implementations/UnaryOperationFactories/` — one factory per operation above.
- `Exceptions/` — `NegativeSquareRootException` (thrown by `SquareRoot` on a negative operand).

### Calculator.BL — business logic
- [Implementations/ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs) — infix→postfix (precedence + associativity; non-mutating).
- [Implementations/ExpressionToCalculatableConverter.cs](Calculator.BL/Implementations/ExpressionToCalculatableConverter.cs) — postfix→tree.
- [Abstractions/ITokenActionHandler.cs](Calculator.BL/Abstractions/ITokenActionHandler.cs) + `Implementations/TokenActionHandlers/` — per-`TokenType` stack handlers and their dispatcher.
- `Exceptions/` — domain exceptions: `MissingOperandException`, `MissingOperatorException`, `OpeningBracketMissingException`, `ClosingBracketMissingException`, `MismatchedBracketPairException`.

(The token model — `Token`, `TokenType`, `BracketPair` — used to live here; it now lives in
`Calculator.Kernel`.)

### Calculator.UI — composition + I/O-facing pipeline
- [Abstractions/ICalculatorUi.cs](Calculator.UI/Abstractions/ICalculatorUi.cs) — the facade front-ends depend on (`Solve(string) : float`).
- [Abstractions/ITokenizer.cs](Calculator.UI/Abstractions/ITokenizer.cs) — `Tokenize(string, Token?) : Token` (takes the previous token for unary/binary disambiguation).
- [Implementations/CalculatorUi.cs](Calculator.UI/Implementations/CalculatorUi.cs) — orchestrates the 3 stages over `IParser`/`IConverter`.
- [Implementations/ExpressionParser.cs](Calculator.UI/Implementations/ExpressionParser.cs), [Implementations/Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs), [Implementations/NumbersValidator.cs](Calculator.UI/Implementations/NumbersValidator.cs).
- [Exceptions/InvalidTokenException.cs](Calculator.UI/Exceptions/InvalidTokenException.cs).
- **[Bootstrapper.cs](Calculator.UI/Bootstrapper.cs) — the composition root.** Defines the splitting
  regex, the operator → factory dictionaries, the bracket pairs, the precedence/associativity map,
  the token handlers, and wires up the entire object graph, returning an `ICalculatorUi`. **This is
  the one place you edit to add an operator or bracket type.**

### Front-ends (all consume `ICalculatorUi`)
- [Calculator.ConsoleApp/Program.cs](Calculator.ConsoleApp/Program.cs) — REPL loop reading lines from the console (supports an `exit` command, guards null input, wraps each evaluation in try/catch).
- [Calculator.WebApi/Controllers/CalculatorController.cs](Calculator.WebApi/Controllers/CalculatorController.cs) — `POST /Calculator`; maps engine errors to **400 BadRequest**; registers the calculator as a singleton in [Program.cs](Calculator.WebApi/Program.cs).
- [Calculator.BlazorUI/](Calculator.BlazorUI/) — Blazor (server host + WASM client) with a button-grid UI in
  [Calculator.BlazorUI.Client/Pages/Calculator.razor](Calculator.BlazorUI/Calculator.BlazorUI.Client/Pages/Calculator.razor);
  the `Calculate` handler wraps `Solve` in try/catch and shows the error message; the calculator is
  injected via DI from the server [Program.cs](Calculator.BlazorUI/Calculator.BlazorUI/Program.cs).

### Calculator.Tests — automated tests
xUnit project (in the solution, targeting net8.0). 41 tests: end-to-end arithmetic, precedence and
associativity, all three bracket styles, every error path, invariant-culture parsing, and a
transformer-level input-immutability regression test.
- [CalculatorEndToEndTests.cs](Calculator.Tests/CalculatorEndToEndTests.cs) — drives `Bootstrapper.Initialize()`.
- [ShuntingYardTransformerTests.cs](Calculator.Tests/ShuntingYardTransformerTests.cs) — transformer unit tests.

## 4. Extension recipes

**Add a binary operator** (say modulo `%`):
1. `Modulo : BinaryOperationBase` in `Calculator.Core/Implementations/BinaryOperations/`.
2. `ModuloFactory : IBinaryOperationFactory` (just `Create`) in `.../BinaryOperationFactories/`.
3. In [Bootstrapper.cs](Calculator.UI/Bootstrapper.cs): one entry in the `binaryOperations`
   dictionary, one entry in the `operationsPrecedence` map (an `OperationPrecedence` with the
   precedence and, if needed, `OperationAssociativity.Right`), and — if the symbol isn't already
   covered — the splitting regex.

**Add a unary function** (e.g. `sin`): same pattern using `UnaryOperationBase` /
`IUnaryOperationFactory`, plus an entry in the `unaryOperations` dictionary and the
`operationsPrecedence` map (add the keyword to the regex's `\b(sqrt|abs)\b` group).

**Add a bracket type:** add a `BracketPair` to the `bracketPairs` list in the `Bootstrapper`
(and the characters to the regex).

**Add a front-end:** call `Bootstrapper.Initialize()` (or DI-register the resulting
`ICalculatorUi`) and call `Solve`.
