# Exercise: The Expression Calculator

Build a calculator that evaluates a math expression given as a string
(e.g. `"2 + sqrt(16) * (3 ^ 2)"`).

Work through the levels in order. Each one adds a requirement on top of the last. Don't read
ahead — treat each level as if it were the whole job.

> A reference implementation lives in this repo; see [CODEMAP.md](CODEMAP.md). Compare *after*
> attempting each level.

---

## General requirements (apply to every level)
- Input is a single string; whitespace within it is ignored.
- Numbers may be decimal.
- Invalid input must produce **specific, meaningful errors** that distinguish different
  failures (e.g. empty input, unknown symbols, malformed expressions, mathematically
  undefined results) — not a single generic error.

---

## Level 1 — Core
Evaluate expressions using `+`, `-`, `*`, `/`, and parentheses `( )`, respecting standard
operator precedence.

## Level 2 — More operators
Add exponentiation `^`.

## Level 3 — Functions
Add the functions `sqrt` and `abs`.

## Level 4 — More bracket styles
Support additional, interchangeable bracket styles such as `[ ]` and `{ }` alongside `( )`.
Brackets must match in kind and nest correctly. Adding a further style later should be
straightforward.

## Level 5 — Multiple front-ends
Expose the same calculator through more than one interface — for example a console prompt, a
web API, and a graphical UI — all driving the same engine.

## Level 6 — Tests
Provide automated tests covering arithmetic, precedence, brackets, and the error cases from
the general requirements.

---

## Going further
Once the levels are done, try adding: a `%` operator; a `sin`/`cos` function; another bracket
style; an additional front-end; a different number type.
