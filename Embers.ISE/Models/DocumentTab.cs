using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Embers.ISE.Models;

public sealed class DocumentTab : INotifyPropertyChanged
{
    private string _text = string.Empty;
    private string? _filePath;
    private string _title;
    private bool _isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public DocumentTab(string title, string? filePath = null, string text = "")
    {
        _title = title;
        _filePath = filePath;
        _text = text;
    }

    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    public string? FilePath
    {
        get => _filePath;
        set => Set(ref _filePath, value);
    }

    public string Text
    {
        get => _text;
        set => Set(ref _text, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
