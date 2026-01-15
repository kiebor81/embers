using Embers.Language;

namespace Embers.Functions;
/// <summary>
/// IFunction interface defines the contract for all functions in the language.
/// </summary>
public interface IFunction
{
    /// <summary>
    /// Applies against the provided context.
    /// </summary>
    /// <param name="self">The self.</param>
    /// <param name="context">The context.</param>
    /// <param name="values">The values.</param>
    /// <returns></returns>
    object Apply(DynamicObject self, Context context, IList<object> values);
}

