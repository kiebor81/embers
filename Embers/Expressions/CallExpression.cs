using Embers.Exceptions;
using Embers.Functions;

namespace Embers.Expressions
{
    /// <summary>
    /// CallExpression represents a function call in the Embers language.
    /// It contains the function name and a list of arguments to be passed to the function.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    public class CallExpression(string name, IList<IExpression> arguments) : IExpression
    {
        private static readonly int hashtag = typeof(CallExpression).GetHashCode();

        private readonly string name = name;
        private readonly IList<IExpression> arguments = arguments;

        //public object Evaluate(Context context)
        //{
        //    IFunction function = context.Self.GetMethod(name) ?? throw new NoMethodError($"undefined method '{name}'");
        //    IList<object> values = [];

        //    foreach (var argument in arguments)
        //        values.Add(argument.Evaluate(context));

        //    return function.Apply(context.Self, context, values);
        //}

        public object Evaluate(Context context)
        {
            IFunction function = context.Self.GetMethod(name)
                ?? throw new NoMethodError($"undefined method '{name}'");

            IList<object> values = [];
            IFunction? block = null;

            // Check for trailing block
            if (arguments.Count > 0 && arguments[^1] is BlockExpression blockExpr)
            {
                block = new BlockFunction(blockExpr);
                arguments.RemoveAt(arguments.Count - 1);  // Remove block from arguments list
            }

            // Check for &block argument syntax
            foreach (var argument in arguments)
            {
                if (argument is BlockArgumentExpression blockArgExpr)
                {
                    // This is a &block argument - extract the IFunction from the Proc
                    object blockValue = blockArgExpr.Evaluate(context);
                    if (blockValue is Embers.Language.Proc proc)
                    {
                        block = proc.GetFunction();
                    }
                    else if (blockValue == null)
                    {
                        block = null;
                    }
                    else
                    {
                        throw new Embers.Exceptions.TypeError($"Expected Proc for block argument, got {blockValue.GetType().Name}");
                    }
                    // Don't add to values - block is passed separately
                }
                else
                {
                    values.Add(argument.Evaluate(context));
                }
            }

            // Use block-aware function call if applicable
            if (function is DefinedFunction df)
                return df.ApplyWithBlock(context.Self, context, values, block);

            // Fallback to standard function call
            return function.Apply(context.Self, context, values);
        }

        public IList<string> GetLocalVariables() => BaseExpression.GetLocalVariables(arguments);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is CallExpression)
            {
                var expr = (CallExpression)obj;

                if (name != expr.name)
                    return false;

                if (arguments.Count != expr.arguments.Count)
                    return false;

                for (var k = 0; k < arguments.Count; k++)
                    if (!arguments[k].Equals(expr.arguments[k]))
                        return false;

                return true;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int result = hashtag + name.GetHashCode();

            foreach (var argument in arguments)
            {
                result *= 17;
                result += argument.GetHashCode();
            }

            return result;
        }
    }
}
