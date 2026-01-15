namespace Embers.Expressions;
/// <summary>
/// INamedExpression interface represents an expression that has a name and a target expression.
/// </summary>
/// <seealso cref="IExpression" />
public interface INamedExpression : IExpression
{
    /// <summary>
    /// Gets the target expression.
    /// </summary>
    /// <value>
    /// The target expression.
    /// </value>
    IExpression TargetExpression { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    string Name { get; }
}
