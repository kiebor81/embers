using Embers.Exceptions;
using Embers.Functions;

namespace Embers.Language;

/// <summary>
/// DynamicClass represents a class in the Embers language.
/// </summary>
public class DynamicClass(DynamicClass @class, string name, DynamicClass superclass = null, DynamicClass parent = null) : DynamicObject(@class)
{
    private readonly string name = name;
    private DynamicClass superclass = superclass;
    private readonly DynamicClass parent = parent;
    private readonly IDictionary<string, IFunction> methods = new Dictionary<string, IFunction>();
    private readonly Context constants = new();

    public DynamicClass(string name, DynamicClass superclass = null)
        : this(null, name, superclass)
    {
    }

    public string Name { get { return name; } }

    public DynamicClass SuperClass { get { return superclass; } }

    public Context Constants { get { return constants; } }

    public string FullName
    {
        get
        {
            if (parent == null)
                return Name;

            return parent.FullName + "::" + Name;
        }
    }

    public void SetInstanceMethod(string name, IFunction method) => methods[name] = method;

    public IFunction? GetInstanceMethod(string name)
    {
        if (methods.TryGetValue(name, out IFunction? value))
            return value;

        if (superclass != null)
            return superclass.GetInstanceMethod(name);

        return null;
    }

    public DynamicObject CreateInstance() => new(this);

    public override IFunction GetMethod(string name) => base.GetMethod(name);

    public IList<string> GetOwnInstanceMethodNames() => [.. methods.Keys];

    public override string ToString() => FullName;

    public void AliasMethod(string newName, string oldName)
    {
        var method = GetInstanceMethod(oldName);

        if (method == null)
            throw new NameError($"method '{oldName}' not found for alias");

        methods[newName] = method;
    }

    internal void SetSuperClass(DynamicClass superclass) => this.superclass = superclass;
}

