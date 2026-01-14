using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Embers.ISE.ViewModels;

public sealed class Commands(Action action, Func<bool>? canExecute = null) : ICommand
{
    private readonly Action _action = action;
    private readonly Func<bool>? _can = canExecute;

    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => _can?.Invoke() ?? true;
    public void Execute(object? parameter) => _action();
}

public sealed class ParameterCommand(Action<object?> action, Func<object?, bool>? canExecute = null) : ICommand
{
    private readonly Action<object?> _action = action;
    private readonly Func<object?, bool>? _can = canExecute;

    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => _can?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _action(parameter);
}

public sealed class AsyncCommand(Func<Task> action) : ICommand
{
    private readonly Func<Task> _action = action;
    private bool _running;

    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !_running;

    public async void Execute(object? parameter)
    {
        if (_running) return;
        _running = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await _action(); }
        finally
        {
            _running = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
