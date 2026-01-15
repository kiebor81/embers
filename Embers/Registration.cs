using Embers.Exceptions;
using Embers.Security;
using Embers.StdLib;
using System.Reflection;

namespace Embers;
/// <summary>
/// Registration class for Embers exceptions and standard library functions.
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

            var instance = (StdFunction)Activator.CreateInstance(type);
            foreach (var name in attr.Names)
            {
                context.Self.Class.SetInstanceMethod(name, instance);
            }
        }
    }
}

