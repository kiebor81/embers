using Embers.Exceptions;
using Embers.Functions;
using Embers.Language;
using Embers.StdLib;
using Embers.Utilities;

namespace Embers.Expressions
{
    /// <summary>
    /// DotExpression represents a method or property access on an object.
    /// Together with AssignDotExpression, it allows for method calls and property access and enables dynamic invocation of methods and properties on objects.
    /// </summary>
    /// <seealso cref="Embers.Expressions.BaseExpression" />
    /// <seealso cref="Embers.Expressions.INamedExpression" />
    public class DotExpression : BaseExpression, INamedExpression
    {
        private static readonly int hashcode = typeof(DotExpression).GetHashCode();

        private readonly IExpression expression;
        private readonly string name;
        private readonly IList<IExpression> arguments;
        private readonly string qname;

        public DotExpression(IExpression expression, string name, IList<IExpression> arguments)
        {
            this.expression = expression;
            this.name = name;
            this.arguments = arguments;

            qname = AsQualifiedName();
        }

        public IExpression TargetExpression { get { return expression; } }

        public IList<IExpression> Arguments { get { return arguments; } }

        public string Name { get { return name; } }

        public override object Evaluate(Context context)
        {
            if (qname != null)
            {
                Type type = TypeUtilities.AsType(qname);

                if (type != null)
                    return type;
            }

            IList<object> values = [];
            var result = expression.Evaluate(context);

            if (result is not DynamicObject)
            {
                NativeClass nclass = (NativeClass)context.GetValue("Fixnum");
                nclass = (NativeClass)nclass.MethodClass(result, null);
                Func<object, IList<object>, object> nmethod = null;

                if (nclass != null)
                    nmethod = nclass.GetInstanceMethod(name);

                if (arguments != null)
                    foreach (var argument in arguments)
                        values.Add(argument.Evaluate(context));

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

        public string? AsQualifiedName()
        {
            if (!char.IsUpper(name[0]))
                return null;

            if (expression is NameExpression)
            {
                string prefix = ((NameExpression)expression).AsQualifiedName();

                if (prefix == null)
                    return null;

                return prefix + "." + name;
            }

            if (expression is DotExpression)
            {
                string prefix = ((DotExpression)expression).AsQualifiedName();

                if (prefix == null)
                    return null;

                return prefix + "." + name;
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is DotExpression)
            {
                var expr = (DotExpression)obj;

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
}
