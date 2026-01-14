// Control signals are used to control the flow of execution in a program.
// They are used to break out of loops, skip iterations, or redo the last operation.
// These signals are typically used in conjunction with control flow statements like loops and conditionals.
// They are not intended to be caught by the user, but rather to be used internally by the framework.

namespace Embers.Signals;
/// <summary>
/// BreakSignal is used to break out of a loop or a control structure.
/// It represents the keyword "break" in the language.
/// </summary>
/// <seealso cref="Exception" />
public class BreakSignal(object? value) : Exception
{
    public object? Value { get; } = value;
}

/// <summary>
/// NextSignal is used to skip the current iteration of a loop and move to the next one.
/// It reprsents the keyword "next" in the language.
/// </summary>
/// <seealso cref="Exception" />
public class NextSignal : Exception
{
    public NextSignal() { }
}

/// <summary>
/// RedoSignal is used to redo the last operation or iteration.
/// It represents the keyword "redo" in the language.
/// </summary>
/// <seealso cref="Exception" />
public class RedoSignal : Exception
{
    public RedoSignal() { }
}


