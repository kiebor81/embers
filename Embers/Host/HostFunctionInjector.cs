using Embers.Security;
using System.Reflection;

namespace Embers.Host;
public static class HostFunctionInjector
{
    /// <summary>
    /// Injects all host functions from the specified assembly.
    /// </summary>
    /// <param name="machine">The machine.</param>
    /// <param name="assembly">The assembly.</param>
    public static void InjectFromAssembly(this Machine machine, Assembly assembly)
    {
        var baseType = typeof(HostFunction);

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !baseType.IsAssignableFrom(type))
                continue;

            var attr = type.GetCustomAttribute<HostFunctionAttribute>();
            if (attr == null) continue;

            var instance = (HostFunction)Activator.CreateInstance(type);
            TypeAccessPolicy.AddType(type.FullName);

            foreach (var name in attr.Names)
            {
                machine.RootContext.Self.Class.SetInstanceMethod(name, instance);
            }
        }
    }

    /// <summary>
    /// Injects all host functions from the calling assembly.
    /// </summary>
    /// <param name="machine">The machine.</param>
    public static void InjectFromCallingAssembly(this Machine machine) => InjectFromAssembly(machine, Assembly.GetCallingAssembly());

    /// <summary>
    /// Injects all host functions from referenced assemblies.
    /// </summary>
    /// <param name="machine">The machine.</param>
    public static void InjectFromReferencedAssemblies(this Machine machine)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        var assemblies = AppDomain.CurrentDomain
                                  .GetAssemblies()
                                  .Where(a => !a.IsDynamic &&
                                              (a == callingAssembly || callingAssembly.GetReferencedAssemblies().Any(r => r.FullName == a.FullName)));

        foreach (var assembly in assemblies)
        {
            InjectFromAssembly(machine, assembly);
        }
    }
}

