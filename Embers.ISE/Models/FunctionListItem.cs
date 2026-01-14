namespace Embers.ISE.Models;

public sealed class FunctionListItem(string name, string comment)
{
    public string Name { get; } = name;
    public string Comment { get; } = comment;
}
