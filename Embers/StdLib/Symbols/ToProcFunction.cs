using Embers.Language;

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
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 1)
                throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

            var symbol = values[0] as Symbol;
            if (symbol == null)
                throw new Exceptions.TypeError("symbol must be a Symbol");

            // Create a Block that calls the method named by the symbol
            var methodName = symbol.Name;
            
            // The block takes one parameter and calls the method on it
            var block = new Block(
                new List<string> { "obj" },
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
                throw new Exceptions.NoMethodError($"undefined method '{methodName}' for nil");

            // Call the method on the object
            if (obj is DynamicObject dynObj)
            {
                var method = dynObj.GetMethod(methodName);
                if (method == null)
                    throw new Exceptions.NoMethodError($"undefined method '{methodName}' for {dynObj.Class.Name}");
                
                return method.Apply(dynObj, context, new List<object>());
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
                        return nativeMethod(obj, new List<object>());
                    }
                }
                
                throw new Exceptions.NoMethodError($"undefined method '{methodName}' for {obj.GetType().Name}");
            }
        }
    }
}
