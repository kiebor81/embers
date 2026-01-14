using Embers.Annotations;
using System;
using System.Windows.Input;

namespace Embers.ISE.ViewModels;

public sealed class FunctionHelpViewModel : ViewModelBase
{
    private readonly Action _close;

    private string _functionName = "";
    public string FunctionName
    {
        get => _functionName;
        set => Set(ref _functionName, value);
    }

    private string _documentation = "Enter a function name and click Lookup.";
    public string Documentation
    {
        get => _documentation;
        set => Set(ref _documentation, value);
    }

    public ICommand LookupCommand { get; }
    public ICommand CloseCommand { get; }

    public FunctionHelpViewModel(Action close, string? initialFunctionName = null)
    {
        _close = close;
        LookupCommand = new Commands(Lookup);
        CloseCommand = new Commands(close);

        if (!string.IsNullOrWhiteSpace(initialFunctionName))
        {
            FunctionName = initialFunctionName.Trim();
            Lookup();
        }
    }

    private void Lookup()
    {
        var name = FunctionName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            Documentation = "Enter a function name.";
            return;
        }

        if (!FunctionScanner.TryGetFunctionDocumentation(name, out var doc))
        {
            Documentation = $"No documentation found for \"{name}\".";
            return;
        }

        var comments = string.IsNullOrWhiteSpace(doc.Comments) ? "n/a" : doc.Comments;
        var arguments = string.IsNullOrWhiteSpace(doc.Arguments) ? "n/a" : doc.Arguments;
        var returns = string.IsNullOrWhiteSpace(doc.Returns) ? "n/a" : doc.Returns;

        Documentation = $"Name: {doc.Name}\nKind: {doc.Kind}\nArguments: {arguments}\nReturns: {returns}\n\n{comments}";
    }
}
