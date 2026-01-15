namespace Embers.Expressions;
/// <summary>
/// Compare operators used in expressions.
/// </summary>
public enum CompareOperator
{
    /// <summary>
    /// The equals operator (=) is used to compare two values for equality. 
    /// </summary>
    Equal = 1,
    /// <summary>
    /// The not equal operator (!=) is used to compare two values for inequality.
    /// </summary>
    NotEqual = 2,
    /// <summary>
    /// The less than operator (<) is used to compare two values to determine if the first is less than the second.
    /// </summary>
    Less = 3,
    /// <summary>
    /// The greater than operator (>) is used to compare two values to determine if the first is greater than the second.
    /// </summary>
    Greater = 4,
    /// <summary>
    /// The less then or equal to operator (<=) is used to compare two values to determine if the first is less than or equal to the second.
    /// </summary>
    LessOrEqual = 5,
    /// <summary>
    /// The greater than or equal to operator (>=) is used to compare two values to determine if the first is greater than or equal to the second.
    /// </summary>
    GreaterOrEqual = 6
}
