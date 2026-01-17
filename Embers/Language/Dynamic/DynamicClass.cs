using Embers.Exceptions;
using Embers.Functions;

namespace Embers.Language.Dynamic;

/// <summary>
/// DynamicClass represents a class in the Embers language.
/// </summary>
public class DynamicClass(DynamicClass @class, string name, DynamicClass superclass = null, DynamicClass parent = null) : DynamicObject(@class)
{
    private readonly string name = name;
    private DynamicClass superclass = superclass;
    private readonly DynamicClass parent = parent;
    private readonly IDictionary<string, IFunction> methods = new Dictionary<string, IFunction>();
    private readonly IList<DynamicClass> mixins = [];
    private readonly Context constants = new();

    public DynamicClass(string name, DynamicClass superclass = null)
        : this(null, name, superclass)
    {
    }

    public string Name { get { return name; } }

    public DynamicClass SuperClass { get { return superclass; } }

    public Context Constants { get { return constants; } }

    public IList<DynamicClass> Mixins => mixins;

    public string FullName
    {
        get
        {
            if (parent == null)
                return Name;

            return parent.FullName + "::" + Name;
        }
    }

    /// <summary>
    /// Sets an instance method for this class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="method"></param>
    public void SetInstanceMethod(string name, IFunction method) => methods[name] = method;

    /// <summary>
    /// Gets an instance method by name, searching mixins and superclass if necessary.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IFunction? GetInstanceMethod(string name)
    {
        if (methods.TryGetValue(name, out IFunction? value))
            return value;

        if (mixins.Count > 0)
        {
            for (int i = mixins.Count - 1; i >= 0; i--)
            {
                var method = mixins[i].GetInstanceMethod(name);
                if (method != null)
                    return method;
            }
        }

        if (superclass != null)
            return superclass.GetInstanceMethod(name);

        return null;
    }

    /// <summary>
    /// Gets an instance method by name, searching mixins but not superclass.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IFunction? GetInstanceMethodNoSuper(string name)
    {
        if (methods.TryGetValue(name, out IFunction? value))
            return value;

        if (mixins.Count > 0)
        {
            for (int i = mixins.Count - 1; i >= 0; i--)
            {
                var method = mixins[i].GetInstanceMethod(name);
                if (method != null)
                    return method;
            }
        }

        return null;
    }

    /// <summary>
    /// Creates an instance of this class.
    /// </summary>
    /// <returns></returns>
    public DynamicObject CreateInstance() => new(this);

    public override IFunction GetMethod(string name) => base.GetMethod(name);

    /// <summary>
    /// Gets the names of all own instance methods.
    /// </summary>
    /// <returns></returns>
    public IList<string> GetOwnInstanceMethodNames() => [.. methods.Keys];

    public override string ToString() => FullName;

    /// <summary>
    /// Creates an alias for an existing method.
    /// </summary>
    /// <param name="newName"></param>
    /// <param name="oldName"></param>
    /// <exception cref="NameError"></exception>
    public void AliasMethod(string newName, string oldName)
    {
        var method = GetInstanceMethod(oldName);

        if (method == null)
            throw new NameError($"method '{oldName}' not found for alias");

        methods[newName] = method;
    }

    /// <summary>
    /// Includes a module (mixin) into this class.
    /// </summary>
    /// <param name="module"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void IncludeModule(DynamicClass module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        if (ReferenceEquals(module, this))
            throw new ArgumentException("module cannot include itself", nameof(module));

        if (mixins.Contains(module))
            mixins.Remove(module);

        mixins.Add(module);
    }

    /// <summary>
    /// Sets the superclass of this class.
    /// </summary>
    /// <param name="superclass"></param>
    internal void SetSuperClass(DynamicClass superclass) => this.superclass = superclass;
}


