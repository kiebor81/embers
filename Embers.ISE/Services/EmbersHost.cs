using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Embers.Host;
using Embers.Security;

namespace Embers.ISE.Services;

public sealed class EmbersHost
{
    public event Action<string>? Stdout;
    public event Action<string>? Stderr;

    private Machine _machine;
    private readonly LineBufferedWriter _writer;
    private readonly List<string> _referenceAssemblies = [];
    private SecurityMode _securityMode = SecurityMode.Unrestricted;
    private readonly List<string> _whitelistEntries = [];

    public EmbersHost()
    {
        _writer = new LineBufferedWriter(line => Stdout?.Invoke(line + "\n"));

        // IMPORTANT: set before Machine init so stdlib captures it
        Console.SetOut(_writer);
        Console.SetError(_writer);

        _machine = CreateMachine();
    }

    public Task<object?> ExecuteAsync(string code, CancellationToken ct)
        => Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            return _machine.ExecuteText(code);
        }, ct);

    public void AddReferenceAssembly(string path)
    {
        var assembly = LoadAssembly(path);
        if (!_referenceAssemblies.Any(p => StringComparer.OrdinalIgnoreCase.Equals(p, path)))
            _referenceAssemblies.Add(path);

        _machine.InjectFromAssembly(assembly);
    }

    public void RemoveReferenceAssembly(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        _referenceAssemblies.RemoveAll(p => StringComparer.OrdinalIgnoreCase.Equals(p, path));
        _machine = CreateMachine();
    }

    public IReadOnlyList<Assembly> GetReferenceAssembliesSnapshot()
        => [.. GetReferenceAssemblies()];

    public void SetSecurityPolicy(SecurityMode mode, IEnumerable<string> whitelistEntries)
    {
        _securityMode = mode;
        _whitelistEntries.Clear();
        if (whitelistEntries != null)
            _whitelistEntries.AddRange(whitelistEntries);

        // Recreate the Machine to avoid stale static policy state.
        _machine = CreateMachine();
    }

    private Machine CreateMachine()
    {
        var machine = new Machine();
        machine.SetTypeAccessPolicy(_whitelistEntries, _securityMode);
        foreach (var assembly in GetReferenceAssemblies())
            machine.InjectFromAssembly(assembly);
        machine.InjectFromReferencedAssemblies();
        machine.InjectFromCallingAssembly();
        return machine;
    }

    private static Assembly LoadAssembly(string path)
    {
        var existing = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a => !a.IsDynamic &&
                                 string.Equals(a.Location, path, StringComparison.OrdinalIgnoreCase));

        return existing ?? Assembly.LoadFrom(path);
    }

    private IEnumerable<Assembly> GetReferenceAssemblies()
    {
        foreach (var path in _referenceAssemblies)
        {
            if (!File.Exists(path)) continue;
            Assembly? assembly;
            try
            {
                assembly = LoadAssembly(path);
            }
            catch (Exception)
            {
                assembly = null;
            }

            if (assembly != null)
                yield return assembly;
        }
    }

    private sealed class LineBufferedWriter(Action<string> emit) : TextWriter
    {
        private readonly Action<string> _emit = emit;
        private readonly Lock _lock = new();
        private readonly System.Text.StringBuilder _sb = new();

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void Write(char value)
        {
            lock (_lock)
            {
                if (value == '\n')
                {
                    _emit(_sb.ToString());
                    _sb.Clear();
                }
                else if (value != '\r')
                {
                    _sb.Append(value);
                }
            }
        }

        public override void Write(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            lock (_lock)
            {
                foreach (var ch in value) Write(ch);
            }
        }

        public override void Flush()
        {
            lock (_lock)
            {
                if (_sb.Length > 0)
                {
                    _emit(_sb.ToString());
                    _sb.Clear();
                }
            }
        }
    }
}
