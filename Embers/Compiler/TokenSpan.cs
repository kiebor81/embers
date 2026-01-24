namespace Embers.Compiler;

/// <summary>
/// Represents the span of a token in the source code.
/// </summary>
/// <param name="StartOffset"></param>
/// <param name="EndOffset"></param>
public readonly record struct TokenSpan(int StartOffset, int EndOffset);
