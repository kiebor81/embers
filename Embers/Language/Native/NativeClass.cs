using Embers.StdLib;

namespace Embers.Language.Native;
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
        return NativeClassResolver.Resolve(machine.RootContext, self);
    }
}

