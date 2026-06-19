# Code Review — Calculator

**Scope:** design & SOLID review + QA/edge-case overview.
**Commit reviewed:** `feature/ai-mds` @ `ff2aef0` ("Merge branch 'dev' into feature/ai-mds").
**Method:** four parallel sub-agent reviews (SOLID, design patterns/architecture, QA, front-ends),
then re-grounded against the live source via `git show` after a mid-review merge changed the tree.
Two QA findings were additionally **confirmed at runtime** by running the console app.

> **Note on volatility.** The working tree changed during this review (a `dev` merge moved HEAD).
> All claims below are verified against `ff2aef0`. At this commit there are **three front-ends**
> (Console, Web API, Blazor) and the engine returns a numeric result. There are **no unit-test
> sources** in the tree (only stale `obj/` artifacts under `Calculator.BL.Tests` /
> `Calculator.Core.Tests`; neither is in the solution).

---

## Executive summary

The **architecture is genuinely strong**: clean inward-pointing layering, a correct Composite
evaluation tree, factory-per-operation, and dictionary/Strategy dispatch make the operator and
bracket sets extensible without touching the engine. Error handling is correctly pushed to the
front-end boundaries for two of the three hosts.

The weaknesses are concentrated in **arithmetic correctness** and **test coverage**: binary
subtraction is broken, `^` is wrongly left-associative, and `sqrt(-x)` silently yields `NaN`
(the first two **confirmed at runtime**). The Blazor front-end has no error handling. There are
no automated tests despite the engine being highly testable.

**Final grade: B− (74/100).** Strong design undercut by core-feature bugs, one unguarded
front-end, and absent tests. Grading logic in [§6](#6-final-grade).

---

## 1. Design strengths (advantages)

- **Clean, acyclic layering**, verified via project references: `Common`→nothing,
  `Core`→nothing, `BL`→Common+Core ([Calculator.BL.csproj](Calculator.BL/Calculator.BL.csproj)),
  `UI`→BL+Common+Core ([Calculator.UI.csproj](Calculator.UI/Calculator.UI.csproj)),
  front-ends→UI only. No inner layer reaches outward.
- **Composite pattern, correctly applied.** The expression is a tree of `ICalculatable`
  ([ICalculatable.cs](Calculator.Core/Abstractions/ICalculatable.cs#L3-L6)) with a leaf
  ([CalculatableNumber.cs](Calculator.Core/Implementations/CalculatableNumber.cs#L5-L18)) and
  composite operations ([BinaryOperationBase.cs](Calculator.Core/Abstractions/BinaryOperationBase.cs#L3-L14)).
  Evaluation is one recursive `Calculate()` — no central evaluator switch.
- **Factory-per-operation + data-driven dispatch** keyed on `TokenType`
  ([TokenActionHandler.cs](Calculator.BL/Implementations/TokenActionHandlers/TokenActionHandler.cs#L9-L19),
  [AdditionFactory.cs](Calculator.Core/Implementations/BinaryOperationFactories/AdditionFactory.cs#L6-L14))
  instead of `if/else` ladders.
- **Small, role-specific abstractions** in `Common`
  ([IParser.cs](Calculator.Common/Abstractions/IParser.cs#L5-L8),
  [IConverter.cs](Calculator.Common/Abstractions/IConverter.cs#L3-L6),
  [IValidator.cs](Calculator.Common/Abstractions/IValidator.cs#L3-L6)), with `IOperationPrecedence`
  split out so the transformer consumes only precedence
  ([IOperationPrecedence.cs](Calculator.Core/Abstractions/IOperationPrecedence.cs#L3-L6)).
- **A real composition root** ([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L15-L66)) wires the
  whole graph; everything else gets constructor injection.
- **A lean engine contract.** `ICalculatorUi.Solve` returns a `float`
  ([ICalculatorUI.cs](Calculator.UI/Abstractions/ICalculatorUI.cs#L5)) and `CalculatorUi.Solve`
  is a clean 3-line orchestration with no presentation or error logic baked in
  ([CalculatorUI.cs](Calculator.UI/Implementations/CalculatorUI.cs#L19-L25)). Formatting and error
  policy live at each front-end, which is the right place.
- **Stateless, singleton-safe engine** — registered as `AddSingleton` in both web hosts
  ([WebApi/Program.cs](Calculator.WebApi/Program.cs#L7), [BlazorUI/Program.cs](Calculator.BlazorUI/Calculator.BlazorUI/Program.cs#L12)).
- **Extensibility is real:** new operator = operation + factory + dictionary entry
  ([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L19-L32)) + regex edit; new bracket style = one
  `BracketPair` (three already configured: `()`, `[]`, `{}` —
  [Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L43-L47)).

---

## 2. SOLID analysis (violations & disadvantages)

### S — Single Responsibility — *good*
Pipeline stages are cleanly separated. Minor: `Tokenizer` both builds its rule table inline and
re-derives `TokenType`s by reflection on every call
([Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs#L28-L41)), making classification order
an implicit dependency on enum declaration order. *Fix:* inject an ordered rule set; cache it.

### O — Open/Closed — *good at the edges, closed in the middle*
Strong for operators/handlers. But `ShuntingYardTransformer.Transform` is a hard `switch` over
`TokenType` ([ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L30-L72))
and `TokenType` is a closed enum ([TokenType.cs](Calculator.BL/Enums/TokenType.cs#L3-L10)); a new
token category forces edits to the enum, the switch, and the tokenizer map. Largely inherent to
Shunting-Yard.

### L — Liskov Substitution
- `ICalculatable` hierarchy is exemplary (full substitutability, no downcasts).
- **`ShuntingYardTransformer` destroys its input**, draining it with `input.RemoveAt(0)`
  ([ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L25-L28)),
  contrary to what the `ITransformer<T>` contract implies
  ([ITransformer.cs](Calculator.Common/Abstractions/ITransformer.cs#L5-L8)). *Fix:* iterate
  without mutating.

### I — Interface Segregation
- **`IBinaryOperationFactory : IOperationPrecedence`** conflates creation with parsing metadata
  ([IBinaryOperationFactory.cs](Calculator.Core/Abstractions/IBinaryOperationFactory.cs#L3-L6)).
  The composition root has to re-split the concerns, casting factories back to
  `IOperationPrecedence` ([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L34-L41)) — evidence
  they don't belong together; unary precedence is effectively dead metadata. *Fix:* keep
  factories to `Create(...)`; register precedence separately.
- **Over-broad constraint** `where T : IEnumerable` on `IParser<T>`/`ITransformer<T>`
  ([IParser.cs](Calculator.Common/Abstractions/IParser.cs#L5)) buys nothing. *Fix:* drop/tighten.

### D — Dependency Inversion
- **Headline violation:** `CalculatorUi` depends on the **concrete** `ExpressionParser` and
  `ExpressionToCalculatableConverter`, not `IParser`/`IConverter`
  ([CalculatorUI.cs](Calculator.UI/Implementations/CalculatorUI.cs#L8-L17)) — blocking mocking and
  inconsistent with the rest of the code. *Fix:* depend on the abstractions.
- **`Bootstrapper.Initialize` returns the concrete `CalculatorUi`**, not `ICalculatorUi`
  ([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L15)).

---

## 3. Design patterns & architecture (disadvantages)

- **Template Method is missed.** `*OperationBase` only stores operands; the "evaluate operands
  then apply" repetition (e.g. [Addition.cs](Calculator.Core/Implementations/BinaryOperations/Addition.cs#L11-L14))
  could be `protected abstract float Apply(...)` with a sealed `Calculate()`, collapsing every
  operation and factory.
- **Control flow via exceptions.** Operation handlers translate `Stack.Pop()`'s
  `InvalidOperationException` into `MissingOperandException`
  ([BinaryOperationTokenHandler.cs](Calculator.BL/Implementations/TokenActionHandlers/BinaryOperationTokenHandler.cs#L18-L28)).
  Prefer an explicit `operands.Count < 2` check.
- **Token model ownership leaks across layers.** `Token`, `TokenType`, `BracketPair` live in
  `Calculator.BL` ([Token.cs](Calculator.BL/Token.cs)) but `Calculator.UI` produces and depends
  on them ([ITokenizer.cs](Calculator.UI/Abstractions/ITokenizer.cs#L1-L8) returns a BL `Token`).
  The shared vocabulary belongs in a kernel both depend on.
- **`System.Data` coupling for an exception** — `ExpressionParser` throws
  `System.Data.InvalidExpressionException` (`using System.Data;`)
  ([ExpressionParser.cs](Calculator.UI/Implementations/ExpressionParser.cs#L4-L29)) despite a
  custom-exception convention existing.
- **The regex is a hidden, duplicated extension point.** Operator/bracket characters are
  hard-coded in the splitting regex ([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L17)) *and*
  in the dictionaries/bracket list. *Fix:* generate the pattern from the registered keys.
- **Hand-rolled wiring instead of host DI.** Both web hosts register a hand-built object
  ([WebApi/Program.cs](Calculator.WebApi/Program.cs#L7)) rather than an `AddCalculator(...)`
  extension over `Microsoft.Extensions.DependencyInjection`.

---

## 4. QA: correctness & edge cases

✅ = runtime-confirmed by running the console app this review.

| # | Input / scenario | Expected | Actual (traced; ✅=ran) | Sev | Evidence |
|---|---|---|---|---|---|
| 1 | `2-3`, `10-4-1` | `-1`, `5` | `-` is registered **unary-only**; postfix `2 3 -` makes unary minus consume only `3`, leaving 2 operands → `MissingOperatorException` ("An operator is missing."). **Subtraction is broken.** ✅ | **High** | [Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L27-L32), [UnaryOperationTokenHandler.cs](Calculator.BL/Implementations/TokenActionHandlers/UnaryOperationTokenHandler.cs#L20-L22) |
| 2 | `2^3^2` | `512` (right-assoc) | Pop condition uses `>=`, so the 2nd `^` pops the 1st → `(2^3)^2 = 64`. ✅ | **High** | [ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L33-L34) |
| 3 | `sqrt(-1)` | clean error | No negative guard; returns `NaN` silently → "...is NaN". | **High** | [SquareRoot.cs](Calculator.Core/Implementations/UnaryOperations/SquareRoot.cs#L11-L14) |
| 4 | `()` (empty brackets) | clean domain error | Postfix empty → converter's terminal `operands.Pop()` throws BCL `InvalidOperationException` ("Stack empty"), surfacing as a **leaky non-domain message** (and crashing Blazor, which has no catch — see §5). | Med | [ExpressionToCalculatableConverter.cs](Calculator.BL/Implementations/ExpressionToCalculatableConverter.cs#L34) |
| 5 | culture/overflow: `3.14` on `de-DE`, `1e39` | consistent | `float.Parse`/`TryParse` use **current culture** while the regex only accepts `.`; comma-decimal locales misparse. Overflow → `Infinity` silently. | Med | [NumbersValidator.cs](Calculator.UI/Implementations/NumbersValidator.cs#L9), [NumberTokenHandler.cs](Calculator.BL/Implementations/TokenActionHandlers/NumberTokenHandler.cs#L11) |
| 6 | `-5`, `-(3+2)`, `3*-2`, `2+-3` | work | All correct (incl. the only way to subtract: `2+-3` → `-1` ✅). | — | [ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L43-L70) |
| 7 | `2+3*4`, `(2+3)*4`, `sqrt(16)` | 14, 20, 4 | All correct ✅. | — | — |
| 8 | `""`, whitespace | clean error | `tokens.Length == 0` → `InvalidExpressionException`. ✔ | — | [ExpressionParser.cs](Calculator.UI/Implementations/ExpressionParser.cs#L26-L29) |
| 9 | `(2+3]`, `(2+3`, `2+3)`, `{2+3}` | specific errors / works | Mismatched/missing-bracket exceptions are correct; `{}` is configured and works. ✔ | — | [ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L55-L83), [Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L43-L47) |
| 10 | `4/0` | clean error | `Division` guards `divider == 0` → `DivideByZeroException`. ✔ | — | [Division.cs](Calculator.Core/Implementations/BinaryOperations/Division.cs#L14-L17) |

### Top breaks (with fixes)
1. **Subtraction (#1) — High.** Register `-` as a binary op (precedence 0) and disambiguate unary
   vs binary minus at tokenize time (unary at start, after another operator, or after `(`).
2. **`^` associativity (#2) — High.** Add an associativity flag; pop on `>` (not `>=`) for
   right-associative operators.
3. **`sqrt` of negative (#3) — High.** Guard `value < 0` and throw a domain exception.
4. **Empty/underflow leak (#4) — Med.** Check `operands.Count != 1` before the terminal pop and
   throw a domain exception.
5. **Culture/overflow (#5) — Med.** Parse with `CultureInfo.InvariantCulture`; reject
   `NaN`/`Infinity`.

---

## 5. Front-ends & error contract

The engine returns a `float` and each front-end owns formatting + error handling — good
separation. Execution differs by host:

**Console — good.** Static composition, formats the result, and wraps `Solve` in try/catch
printing "An error has occured: {message}"
([ConsoleApp1/Program.cs](ConsoleApp1/Program.cs#L9-L17)). *Issues (Med):* `while (true)` with no
exit and `Console.ReadLine()` (nullable) passed straight into `Solve`; on EOF it spins printing
"Value cannot be null." indefinitely (observed). Add a quit command and null guard. Typo "occured".

**Web API — good.** Constructor-injected singleton; the controller try/catches and returns proper
`Ok(result)` / `BadRequest(message)` ([CalculatorController.cs](Calculator.WebApi/Controllers/CalculatorController.cs#L17-L29)),
so invalid input correctly maps to **HTTP 400**. *Issues (Med):* `[FromBody] string` requires a
raw JSON string literal (`"1+2"`, quotes included) — a DTO would be clearer; returns
`result.ToString()` rather than a typed payload.

**Blazor — has no error handling (the weak link).** `Equals()` does
`Display = calculator.Solve(Display).ToString();` with **no try/catch**
([Calculator.razor](Calculator.BlazorUI/Calculator.BlazorUI.Client/Pages/Calculator.razor#L137-L140)),
so any invalid expression (including the broken `-` from the on-screen **Subtract** button,
[L111-114](Calculator.BlazorUI/Calculator.BlazorUI.Client/Pages/Calculator.razor#L111-L114))
throws into the render pipeline and breaks the circuit instead of showing a message. **High.**
Minor: the method named `Equals()` hides `object.Equals`; the `lastOperation` field is unused;
the `√` button appends `sqrt` with no bracket; there are no bracket buttons.

**Consistency gap.** Console and Web API both convert errors to user-facing output; Blazor does
not. Centralizing error→message handling (or restoring a guarded display) would make the three
hosts consistent.

---

## 6. Final grade

### B− (74 / 100)

| Category | Score | Notes |
|---|---|---|
| Architecture & layering | 9.0 / 10 | Clean, acyclic, correct direction. Best aspect. |
| Design patterns | 8.5 / 10 | Composite/Factory/Strategy/DI correct; Template Method missed. |
| SOLID | 7.5 / 10 | Great seams; DIP (concrete deps), ISP (factory+precedence), LSV (input mutation). |
| Correctness (QA) | 5.0 / 10 | Three High bugs; subtraction & `^` associativity confirmed at runtime. |
| Error handling & front-ends | 7.0 / 10 | Console & Web API solid (proper 400); Blazor unguarded. |
| Testing | 3.0 / 10 | No test sources in tree; test projects not in the solution. |
| Code hygiene & consistency | 6.0 / 10 | net6.0 libs, `Ui`/`UI` casing, `_unuaryOperations` typo, `ConsoleApp1` folder, `System.Data` exception. |

**Grading logic.** Weighted for a *design* exercise, so architecture/patterns/SOLID carry the
most weight and rightly lift the score — the extensible engine skeleton is above what the
exercise requires, and pushing error handling to the boundaries is a correct call. The grade is
pulled down by three things that matter regardless of intent: (1) **core-feature correctness** — a
calculator where `2-3` errors and `2^3^2 = 64` cannot sit in the top band, and these are
*runtime-confirmed*, not theoretical; (2) **one front-end (Blazor) with no error handling**, which
crashes on the very inputs its own buttons can produce; and (3) the **absence of tests** for code
that is, ironically, very easy to test. Fixing the three High QA bugs and guarding the Blazor
handler would reach a solid A−; adding a test suite would secure it.

### Minor hygiene findings
- **Naming:** file `ICalculatorUI.cs` vs type `ICalculatorUi`
  ([ICalculatorUI.cs](Calculator.UI/Abstractions/ICalculatorUI.cs#L3)); same for `CalculatorUi`.
- **Typo:** `_unuaryOperations` ([Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs#L13)).
- **Project folder** `ConsoleApp1` vs assembly `Calculator.ConsoleApp`.
- **Target framework** `net6.0` for the libraries/console (out of support) while Blazor targets
  net8.0 — unify on net8.0+ via `Directory.Build.props`.
- **Stale test artifacts:** `Calculator.BL.Tests` / `Calculator.Core.Tests` have `obj/` output but
  no source in the tree and are not in the solution — remove or restore them.

---

## 7. Prioritized action list

1. Fix binary subtraction (#1) and `^` associativity (#2). *(correctness, High — runtime-confirmed)*
2. Guard `sqrt` of negatives (#3). *(correctness, High)*
3. Wrap the Blazor `Equals()` handler in try/catch (or centralize error→message). *(High)*
4. Add a unit-test project (in the solution) covering arithmetic, precedence, brackets, and every
   error path.
5. `CalculatorUi`/`Bootstrapper` → depend on / return the interfaces (DIP).
6. Invariant-culture parsing + reject `NaN`/`Infinity` (#5); guard the terminal pop (#4); console
   exit + null guard.
7. Hygiene: typo, naming, folder rename, unify TFM, split precedence off the factory interface,
   move the token model to a shared kernel.
