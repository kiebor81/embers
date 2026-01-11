# Adding StdLib Functions to Embers

This guide explains how to add new standard library functions to Embers using the automatic registration system.

## Overview

Embers uses a reflection-based system to automatically discover and register standard library functions. Functions can be registered:
- **Globally** - Available on all objects via the root context
- **Type-specific** - Available only on specific types (Array, String, Hash, etc.)

## Creating a New StdLib Function

All StdLib functions should inherit from `StdFunction`, which implements `IFunction` and provides standard behaviour for registration and invocation.

### Step 1: Create Your Function Class

Create a new class that inherits from `StdFunction` in the appropriate namespace:

```csharp
using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("reverse", TargetType = "Array")]
    public class ReverseArrayFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("reverse expects an array argument");

            if (values[0] is IList<object> arr)
            {
                var result = new List<object>(arr);
                result.Reverse();
                return result;
            }

            throw new TypeError("reverse expects an array");
        }
    }
}
```

### Step 2: Use the StdLib Attribute

The `[StdLib]` attribute controls how your function is registered:

#### Global Functions (no TargetType)
```csharp
[StdLib("puts", "print")]
public class PutsFunction : StdFunction
{
    // Available globally: puts("hello") or print("hello")
}
```

#### Type-Specific Functions (Single Type)
```csharp
[StdLib("upcase", TargetType = "String")]
public class UpcaseFunction : StdFunction
{
    // Available on strings: "hello".upcase
}
```

#### Multi-Type Functions (Multiple Types)
```csharp
[StdLib("abs", TargetTypes = new[] { "Fixnum", "Float" })]
public class AbsFunction : StdFunction
{
    // Available on both integers and floats: 5.abs or 3.14.abs
}
```

> **Note**: `TargetType` is intended usage (singular) for a single type. `TargetTypes` was introduced later for handling scenarios like numerics where there is high functional overlap. Both properties are supported for backward compatibility. However, new contributions should prefer `TargetTypes` for consistency, even when registering against a single type. `TargetType` may be deprecated in a future release.

### Supported Target Types

- `"Array"` - Methods for arrays (`DynamicArray`)
- `"String"` - Methods for strings
- `"Hash"` - Methods for hashes
- `"Fixnum"` - Methods for integers
- `"Float"` - Methods for floating-point numbers
- `"Range"` - Methods for ranges
- Add more as needed...

### Example: Numeric Functions

Many mathematical operations work identically on both integers and floats. Rather than creating separate functions or duplicating code, use `TargetTypes` to register the same implementation for both types:

```csharp
using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("abs", TargetTypes = new[] { "Fixnum", "Float" })]
    public class AbsFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("abs expects a numeric argument");

            var value = values[0];
            if (value is int i) return Math.Abs(i);
            if (value is double d) return Math.Abs(d);
            
            throw new TypeError("abs expects a numeric argument");
        }
    }
}
```

Now the `abs` method works on both types:
```ruby
5.abs      # => 5
(-5).abs   # => 5
3.14.abs   # => 3.14
(-3.14).abs # => 3.14
```

Other examples of multi-type numeric functions:
- `ceil`, `floor`, `round` - Rounding operations
- `sin`, `cos`, `tan` - Trigonometric functions
- `sqrt`, `pow` - Exponential operations
- `min`, `max` - Comparison operations

### Step 3: That's It!

Your function will automatically be discovered and registered when the `Machine` is initialized.

## Supporting Blocks

`ApplyWithBlock` is invoked when the Embers method is called with a block; otherwise, `context.Block` may be null.
If your function needs to accept blocks (Ruby closures), override `ApplyWithBlock`. Blocks execute synchronously and share the current execution context unless explicitly isolated by the host.

```csharp
[StdLib("each", TargetType = "Array")]
public class EachFunction : StdFunction
{
    public override object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction block)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("each expects an array argument");

        if (block == null)
            throw new ArgumentError("each expects a block");

        if (values[0] is IEnumerable<object> arr)
        {
            foreach (var item in arr)
                block.Apply(self, context, [item]);
            
            return values[0]; // Return the original array
        }

        throw new TypeError("each expects an array");
    }

    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        return ApplyWithBlock(self, context, values, context.Block);
    }
}
```

## Method Aliases

You can register a function under multiple aliases:

```csharp
[StdLib("length", "len", "size", TargetType = "Array")]
public class LengthFunction : StdFunction
{
    // Available as: arr.length, arr.len, or arr.size
}
```

## Organization

### File Structure
```
Embers/
  StdLib/
    Arrays/          # Array-specific functions
      MapFunction.cs
      SelectFunction.cs
      ...
    Strings/         # String-specific functions
      UpcaseFunction.cs
      DowncaseFunction.cs
      ...
    Numeric/         # Math functions
      AbsFunction.cs
      SqrtFunction.cs
      ...
    Conversion/      # Type conversion functions
      ToIFunction.cs
      ToFFunction.cs
      ...
```

### Naming Conventions
- Function class name: `<MethodName>Function` (e.g., `MapFunction`, `UpcaseFunction`)
- Namespace: `Embers.StdLib.<Category>` (e.g., `Embers.StdLib.Arrays`)
- Ruby method name: snake case (e.g., `"to_s"`, `"start_with"`)

## Testing

Always add tests for your new functions:

### Array Function Example
```csharp
namespace Embers.Tests.StdLib.Arrays
{
    [TestClass]
    public class ReverseFunctionTests
    {
        [TestMethod]
        public void ReverseArray_ReturnsReversedArray()
        {
            Machine machine = new();
            var result = machine.ExecuteText("[1, 2, 3].reverse");
            
            Assert.IsInstanceOfType(result, typeof(List<object>));
            var list = (List<object>)result;
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(3, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(1, list[2]);
        }
    }
}
```

### Multi-Type Function Example
When testing functions with `TargetTypes`, test each type separately:

```csharp
namespace Embers.Tests.StdLib.Numeric
{
    [TestClass]
    public class NumericMethodsIntegrationTests
    {
        [TestMethod]
        public void Fixnum_AbsMethod_NegativeNumber_ReturnsPositive()
        {
            Machine machine = new();
            var result = machine.ExecuteText("x = -5\nx.abs");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void Float_AbsMethod_PositiveNumber_ReturnsSame()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.14.abs");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3.14, result);
        }

        [TestMethod]
        public void Fixnum_CeilMethod_ReturnsInteger()
        {
            Machine machine = new();
            var result = machine.ExecuteText("5.ceil");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void Float_CeilMethod_RoundsUp()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.14.ceil");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result);
        }
    }
}
```

## How It Works

1. **Discovery**: `StdLibRegistry` uses reflection to find all classes decorated with `[StdLib]` attribute
2. **Caching**: Methods are cached by target type in a `Dictionary<string, Dictionary<string, Type>>`
3. **Resolution**: When `DynamicArray.GetMethod(name)` is called, it queries `StdLibRegistry.GetMethod("Array", name)`
4. **Instantiation**: Function instances are lightweight and stateless; per-call instantiation simplifies isolation and avoids shared mutable state.

## Contributing

When contributing new StdLib functions:

1. Create your function in the appropriate namespace
2. Add the `[StdLib]` attribute with the Ruby method name(s)
3. Specify `TargetType` if it's type-specific
4. Implement `Apply` (and optionally `ApplyWithBlock`)
5. When applicable, tests should also verify that invalid input produces the correct Ruby-style exception (`TypeError`, `ArgumentError`, etc.).
6. Update this documentation if adding a new category
