using Embers.StdLib;
using System.Collections;

namespace Embers.Language;
/// <summary>
/// NativeClass resolves Ember runtime types to C# equivalents.
/// </summary>
/// <seealso cref="DynamicObject" />
public class NativeClass : DynamicObject
{
    private readonly string name;
    private readonly Machine machine;
    private readonly IDictionary<string, Func<object, IList<object>, object>> methods = new Dictionary<string, Func<object, IList<object>, object>>();

    private NativeClass fixnumclass;
    private NativeClass floatclass;
    private NativeClass stringclass;
    private NativeClass nilclass;
    private NativeClass falseclass;
    private NativeClass trueclass;
    private NativeClass arrayclass;
    private NativeClass hashclass;
    private NativeClass rangeclass;
    private NativeClass datetimeclass;
    private NativeClass symbolclass;

    public NativeClass(string name, Machine machine)
        : base(null)
    {
        this.name = name;
        this.machine = machine;
        SetInstanceMethod("class", MethodClass);
    }

    public string Name { get { return name; } }

    public void SetInstanceMethod(string name, Func<object, IList<object>, object> method) => methods[name] = method;

    public Func<object, IList<object>, object>? GetInstanceMethod(string name)
    {
        // Check manually registered methods first
        if (methods.TryGetValue(name, out Func<object, IList<object>, object>? value))
            return value;

        // Try StdLibRegistry for this native type
        var stdFunc = StdLibRegistry.GetMethod(Name, name);
        if (stdFunc != null)
        {
            // Create a wrapper that adapts StdFunction to the expected signature
            // StdFunction expects: Apply(DynamicObject self, Context context, IList<object> values)
            // NativeClass expects: Func<object, IList<object>, object>
            return (self, values) =>
            {
                // For StdLib functions, the native value (string, int, etc.) is passed as the first argument
                var args = new List<object> { self };
                if (values != null)
                    args.AddRange(values);

                // Get the context from the machine's root context
                // StdLib functions work with the native value directly in the arguments
                return stdFunc.Apply(machine.RootContext.Self, machine.RootContext, args);
            };
        }

        return null;
    }

    public Func<object, IList<object>, object>? GetInstanceMethod(string name, Context context)
    {
        // Check manually registered methods first
        if (methods.TryGetValue(name, out Func<object, IList<object>, object>? value))
            return value;

        // Try StdLibRegistry for this native type
        var stdFunc = StdLibRegistry.GetMethod(Name, name);
        if (stdFunc != null)
        {
            // Create a wrapper that adapts StdFunction to the expected signature
            // StdFunction expects: Apply(DynamicObject self, Context context, IList<object> values)
            // NativeClass expects: Func<object, IList<object>, object>
            return (self, values) =>
            {
                // For StdLib functions, the native value (string, int, etc.) is passed as the first argument
                var args = new List<object> { self };
                if (values != null)
                    args.AddRange(values);

                // Use the provided context which may contain a block
                return stdFunc.Apply(context.Self, context, args);
            };
        }

        return null;
    }

    public override string ToString() => Name;

    public object? MethodClass(object self, IList<object> values)
    {
        if (self == null)
        {
            nilclass ??= (NativeClass)machine.RootContext.GetLocalValue("NilClass");

            return nilclass;
        }


        if (self is int)
        {
            fixnumclass ??= (NativeClass)machine.RootContext.GetLocalValue("Fixnum");

            return fixnumclass;
        }

        if (self is long)
        {
            fixnumclass ??= (NativeClass)machine.RootContext.GetLocalValue("Fixnum");

            return fixnumclass;
        }

        if (self is double)
        {
            floatclass ??= (NativeClass)machine.RootContext.GetLocalValue("Float");

            return floatclass;
        }

        if (self is float || self is decimal)
        {
            floatclass ??= (NativeClass)machine.RootContext.GetLocalValue("Float");

            return floatclass;
        }

        if (self is string)
        {
            stringclass ??= (NativeClass)machine.RootContext.GetLocalValue("String");

            return stringclass;
        }

        if (self is Symbol)
        {
            symbolclass ??= (NativeClass)machine.RootContext.GetLocalValue("Symbol");

            return symbolclass;
        }

        if (self is DateTime)
        {
            datetimeclass ??= (NativeClass)machine.RootContext.GetLocalValue("DateTime");

            return datetimeclass;
        }

        if (self is bool)
            if ((bool)self)
            {
                trueclass ??= (NativeClass)machine.RootContext.GetLocalValue("TrueClass");

                return trueclass;
            }
            else
            {
                falseclass ??= (NativeClass)machine.RootContext.GetLocalValue("FalseClass");

                return falseclass;
            }

        if (self is IDictionary)
        {
            hashclass ??= (NativeClass)machine.RootContext.GetLocalValue("Hash");

            return hashclass;
        }

        if (self is IList)
        {
            arrayclass ??= (NativeClass)machine.RootContext.GetLocalValue("Array");

            return arrayclass;
        }

        if (self is short || self is byte)
        {
            fixnumclass ??= (NativeClass)machine.RootContext.GetLocalValue("Fixnum");

            return fixnumclass;
        }

        if (self is Proc)
        {
            var procclass = (NativeClass)machine.RootContext.GetLocalValue("Proc");
            return procclass;
        }

        if (self is Range)
        {
            rangeclass ??= (NativeClass)machine.RootContext.GetLocalValue("Range");

            return rangeclass;
        }

        return null;
    }
}

