using Embers.Exceptions;

namespace Embers.Expressions;
/// <summary>
/// ModuloExpression represents the modulo operation between two expressions.
/// </summary>
/// <seealso cref="BinaryExpression" />
public class ModuloExpression(IExpression left, IExpression right)
    : BinaryExpression(left, right)
{
    public override object Apply(object leftvalue, object rightvalue)
    {
        if (leftvalue is long ll && rightvalue is long rl)
            return ll % rl;
        if (leftvalue is long llong && rightvalue is int rint)
            return llong % rint;
        if (leftvalue is int lint && rightvalue is long rlong)
            return lint % rlong;
        if (leftvalue is int li && rightvalue is int ri)
            return li % ri;

        throw new TypeError("Modulo operator requires integer operands");
    }
}
