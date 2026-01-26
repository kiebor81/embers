using Embers.Exceptions;
using Embers.Security;
using Embers.StdLib;
using Embers.Functions;
using Embers.StdLib.Comparable;
using Embers.StdLib.Enumerable;
using Embers.StdLib.Conversion;
using System.Reflection;

namespace Embers;
/// <summary>
/// Registration class for Embers runtime, exceptions and standard library functions.
/// This class provides methods to register all exceptions derived from <see cref="BaseError"/>.
/// In addition, it registers all standard library functions that are decorated with the <see cref="StdLibAttribute"/>.
/// </summary>
internal static class Registration
{
    /// <summary>
    /// Registers all embers exceptions.
    /// </summary>
    /// <param name="context">The context.</param>
    internal static void RegisterAllEmbersExceptions(this Context context)
    {
        var baseType = typeof(BaseError);
        var assembly = baseType.Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
            {
                TypeAccessPolicy.AddType(type.FullName);
                // Register with the simple name (e.g., "ArgumentError")
                context.SetLocalValue(type.Name, type);
            }
        }
    }

    /// <summary>
    /// Registers all StdLib functions decorated with StdLibAttribute.
    /// Now uses StdLibRegistry for automatic discovery and registration.
    /// </summary>
    internal static void RegisterAllStdLibFunctions(this Context context)
    {
        StdLibRegistry.RegisterGlobalFunctions(context);
        RegisterAllStdLibFunctionsGlobally(context);
    }

    /// <summary>
    /// Registers all standard library functions globally in the given context.
    /// </summary>
    /// <param name="context"></param>
    private static void RegisterAllStdLibFunctionsGlobally(Context context)
    {
        var baseType = typeof(StdFunction);
        var assembly = baseType.Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !baseType.IsAssignableFrom(type))
                continue;

            var attr = type.GetCustomAttribute<StdLibAttribute>();
            if (attr == null || attr.Names.Length == 0)
                continue;

            TypeAccessPolicy.AddType(type.FullName);

            var instance = Activator.CreateInstance(type) as StdFunction;
            foreach (var name in attr.Names)
            {
                context.Self.Class.SetInstanceMethod(name, instance);
            }
        }
    }

    /// <summary>
    /// Registers the core classes (BasicObject, Object, Module, Class).
    /// </summary>
    /// <param name="machine">The machine instance.</param>
    /// <returns>CoreClasses record containing the registered classes.</returns>
    internal static Machine.CoreClasses RegisterCoreClasses(this Machine machine)
    {
        var rootcontext = machine.RootContext;
        var basicobjectclass = new DynamicClass("BasicObject", null);
        var objectclass = new DynamicClass("Object", basicobjectclass);
        var moduleclass = new DynamicClass("Module", objectclass);
        var classclass = new DynamicClass("Class", moduleclass);

        rootcontext.SetLocalValue("BasicObject", basicobjectclass);
        rootcontext.SetLocalValue("Object", objectclass);
        rootcontext.SetLocalValue("Module", moduleclass);
        rootcontext.SetLocalValue("Class", classclass);

        basicobjectclass.SetClass(classclass);
        objectclass.SetClass(classclass);
        moduleclass.SetClass(classclass);
        classclass.SetClass(classclass);

        return new Machine.CoreClasses(basicobjectclass, objectclass, moduleclass, classclass);
    }

    /// <summary>
    /// Registers the core modules (Enumerable, Comparable).
    /// </summary>
    /// <param name="machine">The machine instance.</param>
    /// <param name="core">The core classes.</param>
    /// <returns>CoreModules record containing the registered modules.</returns>
    internal static Machine.CoreModules RegisterCoreModules(this Machine machine, Machine.CoreClasses core)
    {
        var rootcontext = machine.RootContext;
        var enumerableModule = new DynamicClass(core.Module, "Enumerable", core.Object);
        var enumerableReduce = new EnumerableReduceFunction();
        enumerableModule.SetInstanceMethod("map", new EnumerableMapFunction());
        enumerableModule.SetInstanceMethod("select", new EnumerableSelectFunction());
        enumerableModule.SetInstanceMethod("reject", new EnumerableRejectFunction());
        enumerableModule.SetInstanceMethod("each_with_index", new EnumerableEachWithIndexFunction());
        enumerableModule.SetInstanceMethod("reduce", enumerableReduce);
        enumerableModule.SetInstanceMethod("inject", enumerableReduce);
        enumerableModule.SetInstanceMethod("any?", new EnumerableAnyFunction());
        enumerableModule.SetInstanceMethod("all?", new EnumerableAllFunction());
        enumerableModule.SetInstanceMethod("find", new EnumerableFindFunction());
        enumerableModule.SetInstanceMethod("to_a", new EnumerableToAFunction());
        rootcontext.SetLocalValue("Enumerable", enumerableModule);

        var comparableModule = new DynamicClass(core.Module, "Comparable", core.Object);
        comparableModule.SetInstanceMethod("between?", new BetweenFunction());
        rootcontext.SetLocalValue("Comparable", comparableModule);

        return new Machine.CoreModules(enumerableModule, comparableModule);
    }

    /// <summary>
    /// Registers the core methods on BasicObject, Object, Module, and Class.
    /// </summary>
    /// <param name="machine">The machine instance.</param>
    /// <param name="core">The core classes.</param>
    internal static void RegisterCoreMethods(this Machine machine, Machine.CoreClasses core)
    {
        core.BasicObject.SetInstanceMethod("class", new LambdaFunction(Machine.GetClass));
        core.BasicObject.SetInstanceMethod("methods", new LambdaFunction(Machine.GetMethods));
        core.BasicObject.SetInstanceMethod("singleton_methods", new LambdaFunction(Machine.GetSingletonMethods));
        core.BasicObject.SetInstanceMethod("instance_variable_get", new LambdaFunction(Machine.InstanceVariableGet));
        core.BasicObject.SetInstanceMethod("instance_variable_set", new LambdaFunction(Machine.InstanceVariableSet));
        core.BasicObject.SetInstanceMethod("instance_variables", new LambdaFunction(Machine.InstanceVariables));
        core.BasicObject.SetInstanceMethod("freeze", new LambdaFunction(Machine.FreezeObject));
        core.BasicObject.SetInstanceMethod("frozen?", new LambdaFunction(Machine.IsFrozenObject));
        core.BasicObject.SetInstanceMethod("send", new SendFunction());

        core.Object.SetInstanceMethod("inspect", new InspectFunction());

        core.Module.SetInstanceMethod("superclass", new LambdaFunction(Machine.GetSuperClass));
        core.Module.SetInstanceMethod("name", new LambdaFunction(Machine.GetName));
        core.Module.SetInstanceMethod("include", new LambdaFunction(Machine.IncludeModule));
        core.Module.SetInstanceMethod("class_variable_get", new LambdaFunction((obj, ctx, vals) => Machine.ClassVariableGet(obj, ctx, vals)!));
        core.Module.SetInstanceMethod("class_variable_set", new LambdaFunction((obj, ctx, vals) => Machine.ClassVariableSet(obj, ctx, vals)!));
        core.Module.SetInstanceMethod("class_variables", new LambdaFunction((obj, ctx, vals) => Machine.ClassVariables(obj, ctx, vals)));
        core.Module.SetInstanceMethod("const_get", new LambdaFunction((obj, ctx, vals) => Machine.ConstGet(obj, ctx, vals)!));
        core.Module.SetInstanceMethod("const_set", new LambdaFunction((obj, ctx, vals) => Machine.ConstSet(obj, ctx, vals)!));
        core.Module.SetInstanceMethod("alias_method", new LambdaFunction((obj, ctx, vals) => Machine.AliasMethod(obj, ctx, vals)));
        core.Module.SetInstanceMethod("define_method", new DefineMethodFunction());

        core.Class.SetInstanceMethod("new", new LambdaFunction(Machine.NewInstance));
    }

    /// <summary>
    /// Registers the native classes (Fixnum, Float, String, etc.).
    /// </summary>
    /// <param name="machine">The machine instance.</param>
    /// <param name="core">The core classes.</param>
    internal static void RegisterNativeClasses(this Machine machine, Machine.CoreClasses core)
    {
        var rootcontext = machine.RootContext;
        var fixnumNative = new FixnumClass(machine);
        var floatNative = new FloatClass(machine);
        var stringNative = new StringClass(machine);
        var symbolNative = new SymbolClass(machine);
        var nilNative = new NilClass(machine);
        var falseNative = new FalseClass(machine);
        var trueNative = new TrueClass(machine);
        var arrayNative = new ArrayClass(machine);
        var hashNative = new HashClass(machine);
        var rangeNative = new RangeClass(machine);
        var datetimeNative = new DateTimeClass(machine);
        var jsonNative = new JsonClass(machine);
        var procNative = new ProcClass(machine);
        var regexpNative = new RegexpClass(machine);

        var structClass = new DynamicClass(core.Class, "Struct", core.Object);
        structClass.SingletonClass.SetInstanceMethod("new", new LambdaFunction(Machine.StructNew));
        rootcontext.SetLocalValue("Struct", structClass);

        rootcontext.SetLocalValue("Fixnum", new NativeClassAdapter(core.Class, "Fixnum", core.Object, null, fixnumNative));
        rootcontext.SetLocalValue("Float", new NativeClassAdapter(core.Class, "Float", core.Object, null, floatNative));
        rootcontext.SetLocalValue("String", new NativeClassAdapter(core.Class, "String", core.Object, null, stringNative));
        rootcontext.SetLocalValue("Symbol", new NativeClassAdapter(core.Class, "Symbol", core.Object, null, symbolNative));
        rootcontext.SetLocalValue("NilClass", new NativeClassAdapter(core.Class, "NilClass", core.Object, null, nilNative));
        rootcontext.SetLocalValue("FalseClass", new NativeClassAdapter(core.Class, "FalseClass", core.Object, null, falseNative));
        rootcontext.SetLocalValue("TrueClass", new NativeClassAdapter(core.Class, "TrueClass", core.Object, null, trueNative));
        rootcontext.SetLocalValue("Array", new NativeClassAdapter(core.Class, "Array", core.Object, null, arrayNative));
        rootcontext.SetLocalValue("Hash", new NativeClassAdapter(core.Class, "Hash", core.Object, null, hashNative));
        rootcontext.SetLocalValue("Range", new NativeClassAdapter(core.Class, "Range", core.Object, null, rangeNative));
        rootcontext.SetLocalValue("DateTime", new NativeClassAdapter(core.Class, "DateTime", core.Object, null, datetimeNative));
        rootcontext.SetLocalValue("JSON", new NativeClassAdapter(core.Class, "JSON", core.Object, null, jsonNative));
        rootcontext.SetLocalValue("Proc", new NativeClassAdapter(core.Class, "Proc", core.Object, null, procNative));
        rootcontext.SetLocalValue("Regexp", new NativeClassAdapter(core.Class, "Regexp", core.Object, null, regexpNative));

        var dateTimeAdapter = rootcontext.GetLocalValue("DateTime");
        rootcontext.SetLocalValue("Time", dateTimeAdapter);
        rootcontext.SetLocalValue("Date", dateTimeAdapter);
    }

    /// <summary>
    /// Registers module mixins into the appropriate classes.
    /// </summary>
    /// <param name="machine">The machine instance.</param>
    /// <param name="modules">The core modules.</param>
    internal static void RegisterModuleMixins(this Machine machine, Machine.CoreModules modules)
    {
        var rootcontext = machine.RootContext;
        if (rootcontext.GetLocalValue("Array") is DynamicClass arrayClass)
            arrayClass.IncludeModule(modules.Enumerable);
        if (rootcontext.GetLocalValue("Hash") is DynamicClass hashClass)
            hashClass.IncludeModule(modules.Enumerable);
        if (rootcontext.GetLocalValue("Range") is DynamicClass rangeClass)
            rangeClass.IncludeModule(modules.Enumerable);

        if (rootcontext.GetLocalValue("Fixnum") is DynamicClass fixnumClass)
            fixnumClass.IncludeModule(modules.Comparable);
        if (rootcontext.GetLocalValue("Float") is DynamicClass floatClass)
            floatClass.IncludeModule(modules.Comparable);
        if (rootcontext.GetLocalValue("String") is DynamicClass stringClass)
            stringClass.IncludeModule(modules.Comparable);
        if (rootcontext.GetLocalValue("Symbol") is DynamicClass symbolClass)
            symbolClass.IncludeModule(modules.Comparable);
        if (rootcontext.GetLocalValue("DateTime") is DynamicClass dateTimeClass)
            dateTimeClass.IncludeModule(modules.Comparable);
    }

    /// <summary>
    /// Registers the root context self object.
    /// </summary>
    /// <param name="machine">The machine instance.</param>
    /// <param name="core">The core classes.</param>
    internal static void RegisterRootContextSelf(this Machine machine, Machine.CoreClasses core)
    {
        var rootcontext = machine.RootContext;
        rootcontext.Self = core.Object.CreateInstance();
        rootcontext.Self.Class.SetInstanceMethod("require", new RequireFunction(machine));
    }
}
