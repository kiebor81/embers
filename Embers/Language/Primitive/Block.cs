using Embers.Exceptions;
using Embers.Expressions;
using Embers.Signals;

namespace Embers.Language.Primitive;
/// <summary>
/// Block represents a block of code that can be executed with a set of arguments.
/// </summary>
public class Block(IList<string> argumentnames, IExpression expression, Context context)
{
    private readonly IList<string> argumentnames = argumentnames;
    private readonly IExpression expression = expression;
    private readonly Context context = context;

    public object Apply(IList<object> arguments)
    {
        if (argumentnames == null || argumentnames.Count == 0)
        {
            try
            {
                return expression.Evaluate(context);
            }
            catch (ReturnSignal)
            {
                throw new InvalidOperationError("return can only be used inside methods");
            }
        }

        BlockContext newcontext = new(context);

        for (int k = 0; k < argumentnames.Count; k++)
            if (arguments != null && k < arguments.Count)
                newcontext.SetLocalValue(argumentnames[k], arguments[k]);
            else
                newcontext.SetLocalValue(argumentnames[k], null);

        try
        {
            return expression.Evaluate(newcontext);
        }
        catch (ReturnSignal)
        {
            throw new InvalidOperationError("return can only be used inside methods");
        }
    }
}
