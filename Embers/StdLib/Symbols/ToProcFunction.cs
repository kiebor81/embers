using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Symbols
{
    /// <summary>
    /// Converts a symbol to a proc that calls the named method on its argument.
    /// Example: :to_s.to_proc.call(5) => "5" (calls 5.to_s)
    /// Used for &:symbol syntax: [1,2,3].map(&:to_s)
    /// </summary>
    [StdLib("to_proc", TargetType = "Symbol")]
    public class ToProcFunction : StdFunction
    {
        [Comments("Converts the symbol to a proc that calls the named method on its argument.")]
        [Arguments(ParamNames = new[] { "symbol" }, ParamTypes = new[] { typeof(Symbol) })]
        [Returns(ReturnType = typeof(Proc))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 1)
                throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

            if (values[0] is not Symbol symbol)
                throw new TypeError("symbol must be a Symbol");

            // Create a Block that calls the method named by the symbol
            var methodName = symbol.Name;
            
            // The block takes one parameter and calls the method on it
            var block = new Block(
                ["obj"],
                new SymbolToProcExpression(methodName),
                context
            );

            return new Proc(block);
        }
    }

    /// <summary>
    /// Internal expression used by Symbol.to_proc to call a method on an object.
    /// </summary>
    internal class SymbolToProcExpression : Expressions.BaseExpression
    {
        private readonly string methodName;

        public SymbolToProcExpression(string methodName)
        {
            this.methodName = methodName;
        }

        public override object Evaluate(Context context)
        {
            // Get the object passed to the proc
            var obj = context.GetLocalValue("obj");
            
            if (obj == null)
                throw new NoMethodError($"undefined method '{methodName}' for nil");

            // Call the method on the object
            if (obj is DynamicObject dynObj)
            {
                var method = dynObj.GetMethod(methodName) ?? throw new NoMethodError($"undefined method '{methodName}' for {dynObj.Class.Name}");
                return method.Apply(dynObj, context, []);
            }
            else
            {
                // Native object - use NativeClass to find the method
                var fixnumClass = context.GetValue("Fixnum");
                if (fixnumClass is NativeClass nclass)
                {
                    var methodClass = nclass.MethodClass(obj, null) as NativeClass;
                    var nativeMethod = methodClass?.GetInstanceMethod(methodName, context);
                    if (nativeMethod != null)
                    {
                        return nativeMethod(obj, []);
                    }
                }
                
                throw new NoMethodError($"undefined method '{methodName}' for {obj.GetType().Name}");
            }
        }
    }
}
