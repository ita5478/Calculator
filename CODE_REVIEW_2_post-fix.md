# Code Review #2 — Calculator (post-fix)

**Scope:** design & SOLID review + QA/edge-case overview, re-run after the fixes from
[CODE_REVIEW_1_initial.md](CODE_REVIEW_1_initial.md).
**State reviewed:** current working tree on `feature/ai-mds` (uncommitted fixes present).
**Method:** four parallel sub-agent reviews (SOLID, design patterns/architecture, QA,
front-ends/tests), grounded in the actual files, plus an **empirical test run**:
`dotnet test` → **41 passed / 0 failed** on net8.0.

> This supersedes [CODE_REVIEW_1_initial.md](CODE_REVIEW_1_initial.md), which was written against the pre-fix code. Items below are
> marked **✓ Fixed** (relative to CODE_REVIEW_1_initial.md), **△ Remaining**, or **＋ New**.

---

## Executive summary

The refactor is excellent. Every High-severity issue from CODE_REVIEW_1_initial.md is genuinely resolved and
backed by regression tests:

- ✓ **Binary subtraction works** (new `Subtraction`/`SubtractionFactory`; unary-vs-binary minus
  disambiguated by context in the tokenizer).
- ✓ **`^` is right-associative** (`2^3^2 = 512`) via a new data-driven `OperationAssociativity`.
- ✓ **`sqrt` of a negative throws a domain exception** (`NegativeSquareRootException`).
- ✓ **Blazor handler is exception-safe**; Console has an exit + null guard; WebApi maps errors to **400**.
- ✓ **Architecture cleaned up**: token model extracted to a new `Calculator.Kernel`; DIP, ISP,
  and the input-mutation LSV issue all fixed; TFM unified to **net8.0** via `Directory.Build.props`.
- ✓ **A real xUnit suite** with targeted regression tests for each fixed bug — **41 passing**.

What remains is polish, not correctness: a missed Template Method, operator definitions
scattered across the bootstrapper + a hidden regex, three compiler-flagged nullable
dereferences, a `System.Data` exception, and some test-breadth/hardening gaps.

**Final grade: A− (90/100).** Grading logic in [§6](#6-final-grade).

---

## 1. Design strengths

- **Clean, strictly-inward layering**, verified via `ProjectReference`s: `Kernel`, `Common`,
  `Core` reference **nothing**; `BL`→(Common,Core,Kernel); `UI`→(BL,Common,Core,Kernel);
  front-ends→UI only ([Calculator.BL.csproj](Calculator.BL/Calculator.BL.csproj#L3-L7),
  [Calculator.UI.csproj](Calculator.UI/Calculator.UI.csproj#L3-L8)).
- ✓ **Token model leak resolved.** `Token`/`TokenType`/`BracketPair` now live in
  [Calculator.Kernel](Calculator.Kernel/Token.cs#L5), depended on by both UI and BL — no more
  UI→BL coupling for the shared vocabulary.
- **Composite** evaluation tree ([ICalculatable.cs](Calculator.Core/Abstractions/ICalculatable.cs#L3-L6),
  [BinaryOperationBase.cs](Calculator.Core/Abstractions/BinaryOperationBase.cs#L5-L11)) — one
  recursive `Calculate()`, no central switch.
- **Strategy/dispatch** for token handling ([TokenActionHandler.cs](Calculator.BL/Implementations/TokenActionHandlers/TokenActionHandler.cs#L17-L20))
  and tokenizer validation ([Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs#L28-L35)).
- **Data-driven precedence + associativity** ([OperationPrecedence.cs](Calculator.Core/Implementations/OperationPrecedence.cs#L5-L14),
  [OperationAssociativity.cs](Calculator.Core/Abstractions/OperationAssociativity.cs#L3-L7))
  consumed by a correct Shunting-Yard pop rule ([ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L89-L101)).
- **Single composition root** returning the interface ([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L17)),
  reused by all three thin front-ends.

---

## 2. SOLID analysis

### S — Single Responsibility — *good*
Pipeline stages and per-token handlers each have one job; precedence was extracted into its own
value type ([OperationPrecedence.cs](Calculator.Core/Implementations/OperationPrecedence.cs#L5)).
△ Minor: `Bootstrapper.Initialize()` is both wiring and configuration data (regex + three
parallel dictionaries) ([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L19-L48)); a single
operator-registry object would consolidate it.

### O — Open/Closed — *good*
New operator = operation + factory + dictionary entries; the new `Subtraction` followed exactly
that path ([Subtraction.cs](Calculator.Core/Implementations/BinaryOperations/Subtraction.cs#L5),
[Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L24)). △ The transformer's `switch (token.Type)`
([ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L28-L70))
is still closed against *new token categories* — acceptable for a fixed grammar.

### L — Liskov Substitution — ✓ Fixed
`ShuntingYardTransformer.Transform` no longer drains its input; it iterates read-only and returns
a fresh collection ([ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L21-L86)),
and there's a unit test asserting input immutability
([ShuntingYardTransformerTests.cs](Calculator.Tests/ShuntingYardTransformerTests.cs#L12-L32)).
△ Minor: `Division`/`SquareRoot` throw from `Calculate()` while peers don't — inherent to partial
math functions; document the throwing contract on
[ICalculatable.cs](Calculator.Core/Abstractions/ICalculatable.cs#L5).

### I — Interface Segregation — ✓ Fixed
`IOperationPrecedence` is no longer carried by the factory interfaces;
[IBinaryOperationFactory.cs](Calculator.Core/Abstractions/IBinaryOperationFactory.cs#L3-L6) /
[IUnaryOperationFactory.cs](Calculator.Core/Abstractions/IUnaryOperationFactory.cs#L3-L6) expose
only `Create(...)`, while precedence/associativity live separately
([IOperationPrecedence.cs](Calculator.Core/Abstractions/IOperationPrecedence.cs#L3-L7)).
△ Remaining: the over-broad `where T : IEnumerable` constraint on
[IParser.cs](Calculator.Common/Abstractions/IParser.cs#L5) and
[ITransformer.cs](Calculator.Common/Abstractions/ITransformer.cs#L5) is decorative — it permits
`T = string` and buys no type safety. *Fix:* drop it or make it `IEnumerable<TItem>`.

### D — Dependency Inversion — ✓ Fixed
`CalculatorUi` now depends on `IParser`/`IConverter`, not concretes
([CalculatorUi.cs](Calculator.UI/Implementations/CalculatorUi.cs#L10-L19)), and
`Bootstrapper.Initialize()` returns `ICalculatorUi`
([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L17)). △ Minor: the composition root is a static
factory inside `Calculator.UI` (which therefore compile-references BL/Core concretes) rather than
an `IServiceCollection` extension; extracting a dedicated bootstrap assembly would be cleaner.

---

## 3. Design patterns & architecture (remaining disadvantages)

- △ **Template Method still missed** (highest-value cleanup). `*OperationBase` only holds operands;
  all 8 operations duplicate the operand-evaluation boilerplate
  ([Addition.cs](Calculator.Core/Implementations/BinaryOperations/Addition.cs#L13),
  [Subtraction.cs](Calculator.Core/Implementations/BinaryOperations/Subtraction.cs#L13)). *Fix:*
  `public float Calculate() => Apply(First.Calculate(), Second.Calculate());` with
  `protected abstract float Apply(...)`. Collapses every operation to one line.
- △ **Factory classes are pure boilerplate.** Each is `new X(...)`; a
  `Dictionary<string, Func<...>>` would remove ~9 trivial classes
  ([AdditionFactory.cs](Calculator.Core/Implementations/BinaryOperationFactories/AdditionFactory.cs#L8-L11)).
- △ **Operator definition is scattered + a hidden regex.** Adding an operator touches three
  bootstrapper dictionaries *and* the untyped splitting-regex literal
  ([Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L19)); a multi-word operator must be hand-added
  to the `\b(sqrt|abs)\b` alternation or it won't tokenize. *Fix:* derive the regex from the
  registered keys / use a single operator registry.
- ＋ **Three compiler-flagged nullable dereferences** in the transformer (build emits CS8602 at
  lines 55, 60, 80): `bracketPairs.FirstOrDefault(...)` is dereferenced unguarded
  ([ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L55-L80)).
  Current flow makes a null unreachable, but it's a latent NRE and a warning in a clean build.
  *Fix:* guard the lookup or throw a domain exception on miss.
- △ **Unguarded dictionary indexing** can throw raw `KeyNotFoundException`: `_handlers[token.Type]`
  ([TokenActionHandler.cs](Calculator.BL/Implementations/TokenActionHandlers/TokenActionHandler.cs#L19))
  and `_precedenceOrder[op]` ([ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L91)).
  An operator registered as a factory but missing from the precedence map fails at runtime. *Fix:*
  `TryGetValue` + domain exception.
- △ **`System.Data` coupling**: empty input throws `System.Data.InvalidExpressionException`
  (`using System.Data;`) ([ExpressionParser.cs](Calculator.UI/Implementations/ExpressionParser.cs#L4-L27))
  instead of a project domain exception. *Fix:* add an `EmptyExpressionException`.
- △ **Dead surface:** unused `BracketPair.IsBracket`
  ([BracketPair.cs](Calculator.Kernel/BracketPair.cs#L14)) and the parameterless
  `OpeningBracketMissingException` ctor
  ([OpeningBracketMissingException.cs](Calculator.BL/Exceptions/OpeningBracketMissingException.cs#L5-L7)).

---

## 4. QA: correctness & edge cases

Traced across 29 scenarios; the five CODE_REVIEW_1_initial.md bugs are all fixed, with no still-broken correctness
found. Highlights (✓ = also covered by a passing test):

| # | Scenario | Expected | Actual | Status | Evidence |
|---|---|---|---|---|---|
| 1 | `2-3`, `10-4-1`, `5-3-2` | -1, 5, 0 | correct; binary `-` left-assoc ✓ | ✓ Fixed | [Subtraction.cs](Calculator.Core/Implementations/BinaryOperations/Subtraction.cs#L13), [ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L99) |
| 2 | `2^3^2` | 512 | right-assoc ✓ | ✓ Fixed | [Bootstrapper.cs](Calculator.UI/Bootstrapper.cs#L45) |
| 3 | `sqrt(-1)` / `sqrt(-4)` | domain error | `NegativeSquareRootException` ✓ | ✓ Fixed | [SquareRoot.cs](Calculator.Core/Implementations/UnaryOperations/SquareRoot.cs#L16) |
| 4 | `()` / `( )` | domain error | `MissingOperandException` (no BCL leak) ✓ | ✓ Fixed | [ExpressionToCalculatableConverter.cs](Calculator.BL/Implementations/ExpressionToCalculatableConverter.cs#L35) |
| 5 | culture (`de-DE`), `1e40`, huge literal | invariant; rejected | InvariantCulture; NaN/Infinity rejected ✓ | ✓ Fixed | [NumbersValidator.cs](Calculator.UI/Implementations/NumbersValidator.cs#L10) |
| 6 | `-5`, `--5`, `3*-2`, `2--3`, `2^-2`, `-2^2` | -5, 5, -6, 5, 0.25, -4 | all correct (standard convention) | OK | [Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs#L52-L74) |
| 7 | `(2+3]`, `[2+3)`, `(2+3`, `2+3)`, `(((1)))` | specific errors / 1 | all correct | OK | [ShuntingYardTransformer.cs](Calculator.BL/Implementations/ShuntingYardTransformer.cs#L55-L80) |
| 8 | `5%2` | unsupported | clean `InvalidTokenException` | OK | [Tokenizer.cs](Calculator.UI/Implementations/Tokenizer.cs#L44) |

**Remaining (Low):**
- ＋ **Computed overflow returns `Infinity` silently** (e.g. `9^99`), whereas an Infinity *literal*
  is rejected — slight inconsistency ([Power.cs](Calculator.Core/Implementations/BinaryOperations/Power.cs#L13)).
  *Fix:* reject non-finite results in `Solve` or each `Calculate()`.
- △ Empty-input error is the `System.Data` type (see §3).

---

## 5. Front-ends & tests

### Front-ends — all CODE_REVIEW_1_initial.md issues fixed
- ✓ **Console**: null guard + `exit` command + per-iteration try/catch; typo corrected to
  "occurred" ([Program.cs](Calculator.ConsoleApp/Program.cs#L8-L23)).
- ✓ **Blazor**: handler renamed `Calculate()` (no longer hides `object.Equals`), wrapped in
  try/catch showing `Error: {message}`; Subtract button works; `sqrt(` appends correctly; the
  unused `lastOperation` field is gone
  ([Calculator.razor](Calculator.BlazorUI/Calculator.BlazorUI.Client/Pages/Calculator.razor#L137-L147)).
- ✓ **WebApi**: catches engine exceptions → **400 BadRequest**
  ([CalculatorController.cs](Calculator.WebApi/Controllers/CalculatorController.cs#L20-L28)).

△ Remaining (Low/Med):
- **DI lifetime** is `AddSingleton` ([WebApi/Program.cs](Calculator.WebApi/Program.cs#L7)); the
  engine is stateless so it's safe, but worth a comment or an explicit immutability guarantee.
- **`[FromBody] string`** forces a JSON-quoted body (`"2+3"`); a request DTO would be clearer
  ([CalculatorController.cs](Calculator.WebApi/Controllers/CalculatorController.cs#L18)).
- **Inconsistent error prefixes** across hosts ("An error has occurred:" / "Error:" / bare); and
  result `.ToString()` isn't invariant-culture, so output may localize the decimal separator even
  though parsing is invariant. *Fix:* a shared formatter + InvariantCulture on output.

### Test suite — strong (41 passing, verified)
- ✓ **xUnit, in the solution, correct references** ([Calculator.Tests.csproj](Calculator.Tests/Calculator.Tests.csproj#L14-L19)).
- ✓ **Regression tests for every fixed bug**: subtraction/unary-minus, `2^3^2==512`,
  `NegativeSquareRootException`, the `()` "no BCL leak" case, culture-invariance, overflow-literal
  rejection ([CalculatorEndToEndTests.cs](Calculator.Tests/CalculatorEndToEndTests.cs#L29-L153)).
- ✓ Meaningful assertions (specific exception types, float tolerance), `[Theory]/[InlineData]`,
  and a transformer-level **input-immutability** unit test
  ([ShuntingYardTransformerTests.cs](Calculator.Tests/ShuntingYardTransformerTests.cs#L12-L32)).

△ Remaining (Low/Med):
- **Culture test mutates global `CurrentCulture`** without disabling parallelization → potential
  flakiness ([CalculatorEndToEndTests.cs](Calculator.Tests/CalculatorEndToEndTests.cs#L130-L144)).
  *Fix:* isolate in a non-parallel collection or inject culture.
- **Breadth gaps:** few fractional-result cases (`10/3`), thin transformer unit tests (only `+`,
  no precedence/bracket cases), and no host-level tests (WebApi 200/400 contract untested).

---

## 6. Final grade

### A− (90 / 100)

| Category | Score | Notes |
|---|---|---|
| Architecture & layering | 9.5 / 10 | Kernel extraction, strictly inward, net8.0 unified. |
| Design patterns | 8.5 / 10 | Composite/Strategy/Factory/DI + data-driven precedence; Template Method still missed; factory boilerplate. |
| SOLID | 9.0 / 10 | DIP/ISP/LSV fixed; only the `IEnumerable` constraint + minor notes remain. |
| Correctness (QA) | 9.0 / 10 | All prior bugs fixed; only silent computed-overflow remains (Low). |
| Error handling & front-ends | 8.5 / 10 | All three hosts handle errors; minor inconsistency/request-shape/culture-format. |
| Testing | 8.0 / 10 | 41 passing with real regression coverage; breadth + culture-test isolation gaps. |
| Code hygiene | 7.5 / 10 | 3 nullable-deref warnings, unguarded dictionary indexing, `System.Data` exception, dead code, stale [CODE_REVIEW_1_initial.md]/[CODEMAP.md]. |

**Grading logic.** This is a large jump from CODE_REVIEW_1_initial.md's B−. Weighted for a design exercise, the
architecture/SOLID/patterns categories carry the most weight and are now near-exemplary: the
token-model leak, DIP, ISP, and LSV issues are all genuinely fixed, layering is strictly inward,
and the framework is unified. Crucially, the three High-severity *correctness* bugs that capped
the previous grade are resolved **and locked in by passing regression tests** — verified here by
`dotnet test` (41/0). What keeps it out of the A/A+ band is entirely polish: a missed Template
Method (real duplication across 8 classes), operator definitions split across dictionaries + a
hidden regex, three compiler-flagged nullable dereferences, unguarded dictionary indexing that can
throw framework exceptions, a `System.Data` exception type, and test-breadth/isolation gaps.
Addressing the Template Method + the nullable/KeyNotFound guards + the regex coupling would move
this to a comfortable A.

---

## 7. Prioritized action list

1. Introduce the Template Method (`abstract float Apply(...)`) to remove operand-eval duplication
   across the 8 operations.
2. Resolve the 3 CS8602 nullable dereferences and switch unguarded `[...]` indexing to
   `TryGetValue` + domain exceptions (transformer + token dispatcher).
3. Generate the splitting regex from the registered operator/bracket keys (kill the hidden,
   hand-synced literal); consider a single operator registry to collapse the parallel dictionaries.
4. Replace `System.Data.InvalidExpressionException` with a project `EmptyExpressionException`;
   reject non-finite computed results.
5. Tests: isolate the culture test from parallelism; add fractional-result, richer transformer,
   and WebApi 200/400 host-level cases.
6. Cleanup: drop dead `BracketPair.IsBracket` / unused exception ctor; remove or regenerate the
   stale `CODE_REVIEW_1_initial.md`/`CODEMAP.md` and the empty `Calculator.BL.Tests/` dir; optionally drop the
   `IEnumerable` generic constraint and extract the composition root to its own assembly.
