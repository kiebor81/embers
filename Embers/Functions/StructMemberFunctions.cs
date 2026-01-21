using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.Functions;

/// <summary>
/// Function that gets the value of a struct member.
/// </summary>
[ScannerIgnore]
internal sealed class StructMemberGetFunction(string memberName) : IFunction
{
    private readonly string memberName = memberName;

    public object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 0)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 0)");

        return self.GetValue(memberName);
    }
}

/// <summary>
/// Function that sets the value of a struct member.
/// </summary>
[ScannerIgnore]
internal sealed class StructMemberSetFunction(string memberName) : IFunction
{
    private readonly string memberName = memberName;

    public object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 1)");

        var value = values[0];
        self.SetValue(memberName, value);
        return value;
    }
}

/// <summary>
/// Function that returns the list of member names of a struct.
/// </summary>
[ScannerIgnore]
internal sealed class StructInitializeFunction(IReadOnlyList<string> members) : IFunction
{
    private readonly IReadOnlyList<string> members = members;

    public object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count > members.Count)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected {members.Count})");

        for (int i = 0; i < values.Count; i++)
            self.SetValue(members[i], values[i]);

        return self;
    }
}

/// <summary>
/// Returns an array of symbols representing the member names of the struct.
/// </summary>
[ScannerIgnore]
internal sealed class StructMembersFunction(IReadOnlyList<string> members) : IFunction
{
    private readonly IReadOnlyList<string> members = members;

    public object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 0)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 0)");

        var result = new DynamicArray();
        foreach (var member in members)
            result.Add(new Symbol(member));

        return result;
    }
}
