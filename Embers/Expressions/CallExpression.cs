using System.Collections;
using Embers.Exceptions;
using Embers.Functions;

namespace Embers.Expressions;
/// <summary>
/// CallExpression represents a function call in the Embers language.
/// It contains the function name and a list of arguments to be passed to the function.
/// </summary>
/// <seealso cref="IExpression" />
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
        IFunction? function = context.Self.GetMethod(name);

        IList<object> values = [];
        IFunction? block = null;
        KeywordArguments? keywordArguments = null;
        var argList = arguments.ToList();

        // Check for trailing block
        if (argList.Count > 0 && argList[^1] is BlockExpression blockExpr)
        {
            block = new BlockFunction(blockExpr);
            argList.RemoveAt(argList.Count - 1);
        }

        foreach (var argument in argList)
        {
            if (argument is BlockArgumentExpression blockArgExpr)
            {
                object blockValue = blockArgExpr.Evaluate(context);
                if (blockValue is Proc proc)
                {
                    block = proc.GetFunction();
                }
                else if (blockValue == null)
                {
                    block = null;
                }
                else
                {
                    throw new TypeError($"Expected Proc for block argument, got {blockValue.GetType().Name}");
                }
                continue;
            }

            if (argument is SplatExpression splat)
            {
                var splatValue = splat.Expression.Evaluate(context);
                if (splatValue is IList list)
                {
                    foreach (var item in list)
                        values.Add(item);
                }
                else
                {
                    throw new TypeError("splat expects an Array");
                }
                continue;
            }

            if (argument is KeywordSplatExpression kwSplat)
            {
                var kwValue = kwSplat.Expression.Evaluate(context);
                if (kwValue is IDictionary dict)
                {
                    var hash = keywordArguments?.Values ?? new DynamicHash();
                    foreach (DictionaryEntry entry in dict)
                        hash[entry.Key] = entry.Value;
                    keywordArguments = new KeywordArguments(hash);
                }
                else
                {
                    throw new TypeError("keyword splat expects a Hash");
                }
                continue;
            }

            values.Add(argument.Evaluate(context));
        }

        if (keywordArguments != null)
            values.Add(keywordArguments);

        if (function == null)
        {
            if (Machine.TryInvokeMethodMissing(context.Self, context, name, values, block, out var methodMissingResult))
                return methodMissingResult;

            throw new NoMethodError($"undefined method '{name}'");
        }

        // Use block-aware function call if applicable
        if (function is DefinedFunction df)
            return df.ApplyWithBlock(context.Self, context, values, block);

        // Fallback to standard function call
        return function.Apply(context.Self, context, values);
    }

    public IList<string> GetLocalVariables() => BaseExpression.GetLocalVariables(arguments);

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is CallExpression expr)
        {
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
