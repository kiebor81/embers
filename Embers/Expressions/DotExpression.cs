using Embers.Exceptions;
using Embers.Functions;
using Embers.StdLib;
using Embers.Utilities;

namespace Embers.Expressions;

/// <summary>
/// DotExpression represents a method or property access on an object.
/// Together with AssignDotExpression, it allows for method calls and property access and enables dynamic invocation of methods and properties on objects.
/// The dot operator is used exclusively for member access, not for namespace/type resolution (use :: for that).
/// </summary>
/// <seealso cref="Embers.Expressions.BaseExpression" />
/// <seealso cref="Embers.Expressions.INamedExpression" />
public class DotExpression(IExpression expression, string name, IList<IExpression> arguments) : BaseExpression, INamedExpression
{
    private static readonly int hashcode = typeof(DotExpression).GetHashCode();

    private readonly IExpression expression = expression;
    private readonly string name = name;
    private readonly IList<IExpression> arguments = arguments;

    public IExpression TargetExpression { get { return expression; } }

    public IList<IExpression> Arguments { get { return arguments; } }

    public string Name { get { return name; } }

    public override object Evaluate(Context context)
    {
        IList<object> values = [];
        var result = expression.Evaluate(context);

        if (result is not DynamicObject)
        {
            var nativeClass = NativeClassResolver.Resolve(context, result);
            if (nativeClass != null)
            {
                var dynamicMethod = nativeClass.GetInstanceMethodNoSuper(name);
                if (dynamicMethod != null)
                {
                    var nativeObj = new NativeObject(nativeClass, result);

                    // Check for trailing block in arguments
                    IFunction? nativeBlock0 = null;
                    var nativeArgList0 = arguments.ToList();

                    if (nativeArgList0.Count > 0 && nativeArgList0[^1] is BlockExpression nativeBlockExpr0)
                    {
                        nativeBlock0 = new BlockFunction(nativeBlockExpr0);
                        nativeArgList0.RemoveAt(nativeArgList0.Count - 1);
                    }

                    // Check for &block argument syntax
                    foreach (var argument in nativeArgList0.ToList())
                    {
                        if (argument is BlockArgumentExpression blockArgExpr)
                        {
                            // This is a &block argument - extract the IFunction from the Proc
                            object blockValue = blockArgExpr.Evaluate(context);
                            if (blockValue is Proc proc)
                            {
                                nativeBlock0 = proc.GetFunction();
                            }
                            else if (blockValue == null)
                            {
                                nativeBlock0 = null;
                            }
                            else
                            {
                                throw new Embers.Exceptions.TypeError($"Expected Proc for block argument, got {blockValue.GetType().Name}");
                            }
                            nativeArgList0.Remove(argument);  // Don't add to values - block is passed separately
                        }
                    }

                    // Evaluate remaining arguments
                    foreach (var argument in nativeArgList0)
                        values.Add(argument.Evaluate(context));

                    // Use block-aware function call if method supports blocks
                    if (nativeBlock0 != null && dynamicMethod is ICallableWithBlock callableWithBlock0)
                        return callableWithBlock0.ApplyWithBlock(nativeObj, context, values, nativeBlock0);

                    // Check if method has ApplyWithBlock method (for StdFunction and DefinedFunction)
                    if (nativeBlock0 != null)
                    {
                        var applyWithBlockMethod = dynamicMethod.GetType().GetMethod("ApplyWithBlock");
                        if (applyWithBlockMethod != null)
                            return applyWithBlockMethod.Invoke(dynamicMethod, [nativeObj, context, values, nativeBlock0]);
                    }

                    // Fallback to standard function call
                    return dynamicMethod.Apply(nativeObj, context, values);
                }
            }

            NativeClass nclass = nativeClass?.NativeClass;

            // Check if this is a StdLib method (which needs block in context)
            // vs a manually registered method (which expects block in values)
            bool isStdLibMethod = nclass != null && nativeClass != null && StdLibRegistry.GetMethod(nativeClass.Name, name) != null;

            // Check for trailing block in arguments
            IFunction? nativeBlock = null;
            var nativeArgList = arguments != null ? arguments.ToList() : [];

            // Only extract block for StdLib methods; manual methods still get block in values
            if (isStdLibMethod && nativeArgList.Count > 0 && nativeArgList[^1] is BlockExpression nativeBlockExpr)
            {
                nativeBlock = new BlockFunction(nativeBlockExpr);
                nativeArgList.RemoveAt(nativeArgList.Count - 1);
            }

            // Check for &block argument syntax
            foreach (var argument in nativeArgList.ToList())
            {
                if (argument is BlockArgumentExpression blockArgExpr)
                {
                    // This is a &block argument - extract the IFunction from the Proc
                    object blockValue = blockArgExpr.Evaluate(context);
                    if (blockValue is Proc proc)
                    {
                        nativeBlock = proc.GetFunction();
                    }
                    else if (blockValue == null)
                    {
                        nativeBlock = null;
                    }
                    else
                    {
                        throw new Embers.Exceptions.TypeError($"Expected Proc for block argument, got {blockValue.GetType().Name}");
                    }
                    nativeArgList.Remove(argument);  // Don't add to values - block is passed separately
                }
            }

            // Create a new context with the block if present
            var nativeContext = nativeBlock != null ? new Context(context, nativeBlock) : context;

            Func<object, IList<object>, object> nmethod = null;

            if (nclass != null)
                nmethod = isStdLibMethod
                    ? nclass.GetInstanceMethod(name, nativeContext)
                    : nclass.GetInstanceMethod(name);

            if (nativeArgList.Count > 0)
                foreach (var argument in nativeArgList)
                    values.Add(argument.Evaluate(nativeContext));

            if (nmethod == null)
            {
                if (result is Type && name == "new")
                    return Activator.CreateInstance((Type)result, [.. values]);

                if (result is Type)
                    return TypeUtilities.InvokeTypeMember((Type)result, name, values);

                return ObjectUtilities.GetValue(result, name, values);
            }

            return nmethod(result, values);
        }

        var obj = (DynamicObject)result;

        // Check for trailing block in arguments
        IFunction? block = null;
        var argList = arguments.ToList();
        
        if (argList.Count > 0 && argList[^1] is BlockExpression blockExpr)
        {
            block = new BlockFunction(blockExpr);
            argList.RemoveAt(argList.Count - 1);
        }

        // Check for &block argument syntax
        foreach (var argument in argList.ToList())
        {
            if (argument is BlockArgumentExpression blockArgExpr)
            {
                // This is a &block argument - extract the IFunction from the Proc
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
                    throw new Embers.Exceptions.TypeError($"Expected Proc for block argument, got {blockValue.GetType().Name}");
                }
                argList.Remove(argument);  // Don't add to values - block is passed separately
            }
        }

        // Evaluate remaining arguments
        foreach (var argument in argList)
            values.Add(argument.Evaluate(context));

        var method = obj.GetMethod(name);

        if (method == null)
        {
            if (Predicates.IsConstantName(name))
                try
                {
                    return ObjectUtilities.GetNativeValue(obj, name, values);
                }
                catch
                {
                }

            throw new NoMethodError(name);
        }

        // Use block-aware function call if method supports blocks
        if (block != null && method is ICallableWithBlock callableWithBlock)
            return callableWithBlock.ApplyWithBlock(obj, context, values, block);

        // Check if method has ApplyWithBlock method (for StdFunction and DefinedFunction)
        if (block != null)
        {
            var applyWithBlockMethod = method.GetType().GetMethod("ApplyWithBlock");
            if (applyWithBlockMethod != null)
                return applyWithBlockMethod.Invoke(method, [obj, context, values, block]);
        }

        // Fallback to standard function call
        return method.Apply(obj, context, values);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is DotExpression expr)
        {
            if (arguments.Count != expr.arguments.Count)
                return false;

            for (var k = 0; k < arguments.Count; k++)
                if (!arguments[k].Equals(expr.arguments[k]))
                    return false;

            return name.Equals(expr.name) && expression.Equals(expr.expression);
        }

        return false;
    }

    public override int GetHashCode()
    {
        int result = name.GetHashCode() + expression.GetHashCode() + hashcode;

        foreach (var argument in arguments)
            result += argument.GetHashCode();

        return result;
    }
}
