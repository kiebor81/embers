using Embers.Exceptions;
using Embers.Functions;
using Embers.Language;

namespace Embers.Expressions
{
    /// <summary>
    /// CallOverloadedExpression represents a call to an overloaded function or method in the Embers language.
    /// It contains a target expression that resolves to a callable object (like a delegate or block)
    /// and differs from a standard CallExpression by allowing for multiple arguments beyond the scope of the original callable.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    [Obsolete("this functionality exists in CallExpression. Class reatined for potential future repurposing")]
    public class CallOverloadedExpression(IExpression target, IList<IExpression> arguments) : IExpression
    {
        private static readonly int hashtag = typeof(CallExpression).GetHashCode();

        private readonly IExpression target = target;
        private readonly IList<IExpression> arguments = arguments;

        public object Evaluate(Context context)
        {
            // Evaluate the target to get a callable object (delegate, block, etc.)
            var callable = target.Evaluate(context);

            IList<object> values = [];
            foreach (var argument in arguments)
                values.Add(argument.Evaluate(context));

            // You may need to adapt this to your callable model
            if (callable is IFunction function)
                return function.Apply(context.Self, context, values);
            if (callable is Delegate del)
                return del.DynamicInvoke([.. values]);
            if (callable is Block block)
                return block.Apply(values);

            throw new NoMethodError("Target is not callable");
        }

        public IList<string> GetLocalVariables() => BaseExpression.GetLocalVariables(arguments);

        public override bool Equals(object obj)
        {
            if (obj is not CallOverloadedExpression other)
                return false;
            return target.Equals(other.target) && arguments.SequenceEqual(other.arguments);
        }

        public override int GetHashCode()
        {
            int result = hashtag + target.GetHashCode();
            foreach (var argument in arguments)
            {
                result *= 17;
                result += argument.GetHashCode();
            }
            return result;
        }
    }
}
