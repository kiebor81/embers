# Contributing to Embers

Thank you for your interest in contributing to Embers.

Embers is an embeddable, Ruby-inspired runtime with a deliberately scoped feature set. Contributions are welcome, but they are expected to align with the project’s architectural goals, terminology, and design philosophy.

Please read this document carefully before opening a pull request.

---

## Guiding Principles

Before contributing, please ensure your work aligns with the following principles:

- **Embedding First**  
  Embers is designed to be embedded in host applications. Changes should not assume standalone execution.

- **Explicit Over Implicit**  
  Clear intent is preferred over convenience. Avoid heuristic or “magic” behaviour unless explicitly documented.

- **Ruby-Inspired, Not Ruby-Compatible**  
  Embers implements a Ruby-like language, not Ruby itself. Semantic parity with MRI is not a goal. However, useful language features and standard library additions that align with Embers’ scope are welcome.

- **Minimalism**  
  New features should earn their place. Avoid expanding scope without strong justification.

---

## What You Can Contribute

Contributions are welcome in the following areas:

- Bug fixes
- Performance improvements
- Documentation improvements
- StdLib functions
- Tests (especially edge cases and error handling)
- Internal refactors that improve clarity or maintainability
- Practical examples and *quick start* integrations

Large feature additions should be discussed in an issue before implementation.

---

## Code Style & Architecture

- Follow existing project structure and naming conventions
- Prefer readability over cleverness
- Avoid introducing unnecessary abstractions
- Keep public APIs minimal and well-documented
- Maintain strict separation between:
  - Compiler
  - Language model
  - Host integration
  - Security
  - Standard Library

If you are unsure where something belongs, open an issue first.

---

## Tests (Required)

**All public methods must have test coverage.**

Pull requests that introduce new behaviour without tests will not be accepted.

When applicable, tests should also verify:

- Error conditions
- Correct exception types (`TypeError`, `ArgumentError`, etc.) where applicable
- Boundary cases

Run the full test suite before submitting:

```bash
dotnet test Embers.Tests
```

---

## StdLib Contributions

If you are contributing to the standard library:

- Inherit from `StdFunction`
- Use the `[StdLib]` attribute
- Prefer `TargetTypes` over `TargetType`
- Follow existing naming and namespace conventions
- Add tests for each supported type

See [STDLIB.md](STDLIB.md) for detailed guidance.

---

## Documentation

If your change affects behaviour, APIs, or contributor workflow:

- Update README.md, Examples.md, or STDLIB.md as appropriate
- Keep terminology consistent (use Embers script, Ruby-like, etc.)
- Avoid implying Ruby compatibility or parity

## Submitting a Pull Request

Before submitting a PR, ensure that:

- The code builds successfully
- All tests pass
- New functionality is tested
- Documentation is updated where necessary

Pull requests should have a clear, focused purpose. Large, unfocused PRs may be closed or requested to be split.

--- 

## Questions & Discussion

If you are unsure about a contribution or want feedback before writing code, please open an issue to discuss it first.

