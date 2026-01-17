using Embers.Exceptions;
using Embers.Functions;

namespace Embers.Language.Primitive;
/// <summary>
/// Proc represents a first-class procedure/lambda object in Embers.
/// It can wrap either a Block (from lambdas) or an IFunction (from block parameters).
/// </summary>
public class Proc
{
    private readonly Block? block;
    private readonly IFunction? function;

    public Proc(Block block)
    {
        this.block = block;
        function = null;
    }

    public Proc(IFunction function)
    {
        block = null;
        this.function = function;
    }

    /// <summary>
    /// Call the proc with the given arguments
    /// </summary>
    public object Call(IList<object> arguments)
    {
        if (block != null)
        {
            return block.Apply(arguments);
        }
        else if (function is BlockFunction blockFunc)
        {
            // BlockFunction needs self and context - use null self and empty context
            return blockFunc.Apply(null!, new Context(), arguments);
        }
        else if (function != null)
        {
            throw new NotSupportedError($"Proc does not support function type {function.GetType().Name}");
        }
        else
        {
            throw new InvalidOperationError("Proc has neither block nor function");
        }
    }

    /// <summary>
    /// Get the underlying IFunction to pass as a block parameter.
    /// This is used when passing a proc to another method using &proc syntax.
    /// </summary>
    public IFunction? GetFunction()
    {
        if (function != null)
        {
            return function;
        }
        else if (block != null)
        {
            // Wrap the block in a BlockAdapter so it can be passed as an IFunction
            return new BlockAdapter(block);
        }
        return null;
    }
}

