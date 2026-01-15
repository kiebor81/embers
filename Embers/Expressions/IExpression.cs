namespace Embers.Expressions;
/// <summary>
/// IExpression interface represents an expression in the Embers language.
/// </summary>
public interface IExpression
{
    /// <summary>
    /// Evaluates against the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    object? Evaluate(Context context);

    /// <summary>
    /// Gets the local variables.
    /// </summary>
    /// <returns></returns>
    IList<string>? GetLocalVariables();
}
