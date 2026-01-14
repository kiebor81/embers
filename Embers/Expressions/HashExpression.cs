using Embers.Language;
using System.Collections;

namespace Embers.Expressions
{
    /// <summary>
    /// HashExpression represents a hash expression in the Embers language.
    /// A hash expression consists of key-value pairs, where both keys and values are evaluated expressions.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    public class HashExpression(IList<IExpression> keyexpressions, IList<IExpression> valueexpressions) : IExpression
    {
        private static readonly int hashtag = typeof(HashExpression).GetHashCode();

        private readonly IList<IExpression> keyexpressions = keyexpressions;
        private readonly IList<IExpression> valueexpressions = valueexpressions;

        public object Evaluate(Context context)
        {
            IDictionary result = new DynamicHash();

            for (var k = 0; k < keyexpressions.Count; k++)
                result[keyexpressions[k].Evaluate(context)] = valueexpressions[k].Evaluate(context);

            return result;
        }

        public IList<string> GetLocalVariables() => BaseExpression.GetLocalVariables(valueexpressions);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is HashExpression)
            {
                var expr = (HashExpression)obj;

                if (keyexpressions.Count != expr.keyexpressions.Count)
                    return false;

                if (valueexpressions.Count != expr.valueexpressions.Count)
                    return false;

                for (var k = 0; k < keyexpressions.Count; k++)
                    if (!keyexpressions[k].Equals(expr.keyexpressions[k]))
                        return false;

                for (var k = 0; k < valueexpressions.Count; k++)
                    if (!valueexpressions[k].Equals(expr.valueexpressions[k]))
                        return false;

                return true;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int result = hashtag;

            foreach (var expr in keyexpressions)
            {
                result *= 17;
                result += expr.GetHashCode();
            }

            foreach (var expr in valueexpressions)
            {
                result *= 7;
                result += expr.GetHashCode();
            }

            return result;
        }
    }
}
