# Embers Language Grammar Guide

This guide is intended for users writing Embers scripts and does not describe internal compiler implementation details. It describes the syntax and grammar of the Embers scripting language. Embers draws inspiration from Ruby’s syntax and expression model, but deliberately implements a smaller language-set with its own constraints and behaviour.

## Table of Contents

1. [Basic Syntax](#basic-syntax)
2. [Comments](#comments)
3. [Data Types](#data-types)
4. [Variables](#variables)
5. [Operators](#operators)
6. [Control Flow](#control-flow)
7. [Methods and Functions](#methods-and-functions)
8. [Classes](#classes)
9. [Modules](#modules)
10. [Blocks and Iterators](#blocks-and-iterators)
11. [String Interpolation](#string-interpolation)
12. [Notes on Ruby Compatibility](#notes-on-ruby-compatibility)

## Basic Syntax

Embers programs are written as sequences of expressions separated by newlines or semicolons. As in Ruby, most constructs are expressions, though some statements exist primarily for control flow.

```
x = 5
y = 10
puts x + y
```

Semicolons can be used to separate multiple expressions on a single line:

```
x = 5; y = 10; puts x + y
```

## Comments

Comments begin with `#` and extend to the end of the line:

```
# This is a comment
x = 5  # Comments can appear at the end of a line too
```

## Data Types

### Numbers

**Integers:** Whole numbers without a decimal point. Numbers are arbitrary-sized; there is no distinction between `int` and `long`.

```
42
-15
1000000
```

**Floats/Reals:** Numbers with a decimal point.

```
3.14
-0.5
2.0
```

**Exponentiation:** Use `**` to raise a number to a power.

```
2 ** 8        # equals 256
5 ** -1       # equals 0.2
```

### Strings

Strings are sequences of characters enclosed in quotes.

**Single-quoted strings** treat content literally:

```
'hello world'
'This is a string'
```

**Double-quoted strings** support interpolation. Interpolated expressions may span multiple lines and can contain arbitrary Embers expressions:

```
"Hello, #{name}!"
"2 + 2 = #{2 + 2}"
```

### Symbols

Symbols are immutable identifiers prefixed with a colon. They behave as lightweight values rather than object instances. They’re often used as hash keys or lightweight identifiers passed to methods:

```
:name
:age
:success
```

### Arrays

Arrays are ordered collections of values enclosed in square brackets:

```
[1, 2, 3]
['apple', 'banana', 'cherry']
[1, 'two', 3.0, :four]
```

### Hashes

Hashes are collections of key-value pairs enclosed in curly braces. Keys are typically symbols or strings:

```
{ name: 'Alice', age: 30 }
{ :name => 'Bob', :age => 25 }
{ 'key' => 'value' }
```

**Note:** The `key: value` syntax is shorthand for `:key => value`.
This shorthand is limited to symbol-like keys and does not support arbitrary expressions on the left-hand side.

### Ranges

Ranges represent a sequence of values using the `..` operator:

```
1..10          # Range from 1 to 10 (inclusive)
'a'..'z'       # Range from 'a' to 'z'
```

## Variables

### Local Variables

Local variables begin with a lowercase letter or underscore and can contain letters, digits, and underscores:

```
name = 'Alice'
age = 30
_internal = true
```

Local variables are scoped to their enclosing block or method. Block scoping follows Embers’ execution model and may differ from Ruby in edge cases.

### Instance Variables

Instance variables begin with `@` and belong to an object instance:

```
@name = 'Alice'
@age = 30
@enabled = true
```

Instance variables are only accessible within instance methods of a class.

### Class Variables

Class variables begin with `@@` and are shared by all instances of a class:

```
@@count = 0
@@instances = []
```

## Operators

### Arithmetic Operators

```
+       Addition
-       Subtraction
*       Multiplication
/       Division
%       Modulo (remainder)
**      Exponentiation
```

### Comparison Operators

```
==      Equal to
!=      Not equal to
<       Less than
>       Greater than
<=      Less than or equal to
>=      Greater than or equal to
```

### Logical Operators

```
&&      Logical AND
||      Logical OR
!       Logical NOT
```

### Assignment Operators

```
=       Basic assignment
+=      Add and assign:   x += 2  is x = x + 2
-=      Subtract and assign
*=      Multiply and assign
/=      Divide and assign
%=      Modulo and assign
**=     Exponentiation and assign
```

### Range Operator

```
..      Range (inclusive)
```

### Other Operators

```
.       Method call / member access
::      Namespace separator
=>      Hash literal mapping (key => value)
->      Lambda/proc literal
```

### Operator Precedence

Operators are evaluated in this order (highest to lowest precedence):

1. `**` (exponentiation)
2. `*`, `/`, `%` (multiplication, division, modulo)
3. `+`, `-` (addition, subtraction)
4. `..` (range)
5. `==`, `!=`, `<`, `>`, `<=`, `>=` (comparison)
6. `&&` (logical AND)
7. `||` (logical OR)
8. `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `**=` (assignment)

Parentheses `()` can be used to override precedence:

```
2 + 3 * 4       # equals 14 (multiplication first)
(2 + 3) * 4     # equals 20 (addition first)
```

Operator precedence follows a Ruby-like model but is defined explicitly by the Embers parser.

## Control Flow

### If/Elsif/Else

Execute code conditionally:

```
if age >= 18
  puts "You are an adult"
elsif age >= 13
  puts "You are a teenager"
else
  puts "You are a child"
end
```

The `then` keyword is optional after `if` and `elsif`:

```
if x > 0 then
  puts "Positive"
end
```

### While Loops

Execute a block repeatedly `while` a condition is true:

```
while x < 10
  puts x
  x = x + 1
end
```

The `do` keyword is optional:

```
while x < 10 do
  puts x
  x = x + 1
end
```

### Until Loops

Execute a block repeatedly `until` a condition becomes true:

```
until x >= 10
  puts x
  x = x + 1
end
```

### For Loops

Iterate over a range or collection:

```
for i in 1..5
  puts i
end

for name in ['Alice', 'Bob', 'Charlie']
  puts name
end
```

The `do` keyword is optional:

```
for i in 1..5 do
  puts i
end
```

### Break and Return

Exit from a loop early:

```
while true
  if should_exit
    break
  end
end
```

Exit from a method:

```
def find_value
  return 42 if condition
  return -1
end
```

## Methods and Functions

### Defining Methods

Define methods with the `def` keyword:

```
def greet(name)
  puts "Hello, #{name}!"
end

def add(a, b)
  a + b
end
```

Methods implicitly return the value of their last expression. The `return` keyword can be used for explicit early returns:

```
def is_adult(age)
  if age >= 18
    return true
  else
    return false
  end
end
```

This includes conditional and block expressions.

### Method Naming Conventions

Methods may optionally end with `?` or `!` to indicate their behavior.

- Methods ending in `?` typically return true/false (predicate methods):
  ```
  def valid?
    # returns true or false
  end
  ```

- Methods ending in `!` typically modify the object in-place or have significant side effects (bang methods):
  ```
  def delete!
    # modifies the object or has side effects
  end
  ```

### Calling Methods

Call methods using dot notation:

```
result = object.method_name(arg1, arg2)
```

Parentheses are optional when there are no arguments or when the method call is unambiguous:

```
puts "hello"        # parens omitted
object.method_name  # parens omitted for no args
```

### Instance Methods

Instance methods are defined without the `self.` prefix and operate on instance data:

```
class Calculator
  def add(a, b)
    a + b
  end
end

calc = Calculator.new
calc.add(5, 3)
```

### Class Methods

Class methods are defined with the `self.` prefix:

```
class Utils
  def self.current_time
    Time.now
  end
end

Utils.current_time
```

## Classes

### Defining Classes

Define classes with the `class` keyword. Class names must start with an uppercase letter:

```
class Dog
  def initialize(name)
    @name = name
  end

  def bark
    puts "#{@name} says woof!"
  end
end
```

### Constructor

The `initialize` method acts as the constructor when creating a new instance:

```
dog = Dog.new("Rex")
dog.bark               # prints: Rex says woof!
```

### Instance Variables

Instance variables belong to each instance and are initialized in `initialize`:

```
class Person
  def initialize(name, age)
    @name = name
    @age = age
  end

  def introduce
    puts "I'm #{@name}, #{@age} years old"
  end
end
```

### Class Variables

Class variables are shared among all instances:

```
class Counter
  @@count = 0

  def initialize
    @@count = @@count + 1
  end

  def self.total
    @@count
  end
end
```

### Inheritance

Classes can inherit from other classes using `<`:

```
class Animal
  def speak
    puts "Some sound"
  end
end

class Cat < Animal
  def speak
    puts "Meow"
  end
end
```

## Modules

### Defining Modules

Modules are collections of related methods and constants:

```
module Greeting
  def hello
    puts "Hello!"
  end

  def goodbye
    puts "Goodbye!"
  end
end
```

### Using Modules

Modules can be included in classes to mix in their methods:

```
class Person
  include Greeting
end

person = Person.new
person.hello      # prints: Hello!
```

## Blocks and Iterators

### Block Syntax

Blocks are chunks of code that can be passed to methods. They use either `do...end` or `{...}`:

```
[1, 2, 3].each do |x|
  puts x
end

# or with braces:
[1, 2, 3].each { |x| puts x }
```

The `|x|` syntax declares block parameters. Block parameters are local to the block body

### Passing Blocks to Methods

Define methods that accept blocks:

```
def repeat(times)
  i = 0
  while i < times
    yield
    i = i + 1
  end
end

repeat(3) do
  puts "Hello"
end
```

The `yield` keyword calls the block.

### Block Arguments

Pass data to a block using `yield`:

```
def each_item(items)
  for item in items
    yield item
  end
end

each_item(['a', 'b', 'c']) do |item|
  puts item
end
```

## String Interpolation

Double-quoted strings support interpolation using `#{...}`:

```
name = "Alice"
age = 30

puts "Name: #{name}"
puts "Next year, #{name} will be #{age + 1}"
```

Expressions inside `#{}` are evaluated and converted to strings.

Single-quoted strings do not support interpolation:

```
puts 'Name: #{name}'     # prints literally: Name: #{name}
```

---

## Notes on Ruby Compatibility

Embers draws from Ruby's design but is a distinct language with its own implementation. Embers prioritizes embeddability and clarity over full Ruby feature parity. 

While syntax is often similar to Ruby, there are differences in:

- Standard library and built-in methods
- Exact semantics of certain operations
- Available classes and modules

Refer to the Embers documentation for specifics on the standard library and available methods.
