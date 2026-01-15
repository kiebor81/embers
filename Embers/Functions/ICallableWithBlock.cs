using Embers.Language;

namespace Embers.Functions;
/// <summary>
/// ICallableWithBlock interface extends the IFunction interface to allow functions that can be called with a block.
/// CallableWithBlock functions can accept an additional block parameter that can be executed within the function's context.
/// </summary>
/// <seealso cref="IFunction" />
public interface ICallableWithBlock : IFunction
{
    /// <summary>
    /// Applies the block context.
    /// </summary>
    /// <param name="self">The self.</param>
    /// <param name="caller">The caller.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="block">The block.</param>
    /// <returns></returns>
    object ApplyWithBlock(DynamicObject self, Context caller, IList<object> args, IFunction? block);
}


