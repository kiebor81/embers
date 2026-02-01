# Examples

Embers can be used in a multitude of ways. The repository contains a REPL/CLI example you can use to explore host integration.

## CLI Usage (Example)

From `Embers.Console`:

```bash
dotnet run --project Embers.Console "script.rb"
```

Or launch the REPL:

```bash
dotnet run --project Embers.Console
```

Exit the REPL using `exit`, `quit`, or `bye` commands.

---

## Use Cases

The following are detailed, real-world scenarios demonstrating how to integrate Embers into your projects. These examples demonstrate advanced embedding scenarios. 

---

### 1. Game Scripting in Godot (Dynamic Node Interaction & Autoloads)

For game engines like Godot and Unity, Embers doesn't give you Ruby syntax to replace built-in language support, but it does give you a boundary between developer code and user code.

**Use Case**: Enable game designers to script behaviours, quest logic, and NPC interactions in Embers without recompiling the game.

A fully functional Embers scripted game can be downaloaded from [itch.io](https://sad-dragons.itch.io/boing)

<iframe frameborder="0" src="https://itch.io/embed/4242898?border_width=4&amp;bg_color=242424&amp;fg_color=eef4e8&amp;link_color=8cfa5b&amp;border_color=41823f" width="558" height="173"><a href="https://sad-dragons.itch.io/boing">BOING! by sad-dragons</a></iframe>

### 2. User-Generated Content in .NET Desktop Applications

**Use Case**: Allow end-users to create plugins, mods, or automation scripts for your desktop application.

#### Building a User-Friendly DSL for a File Organizer Tool

```csharp
using Embers;
using Embers.Host;
using System.IO;

public class FileOrganizerApp
{
    private Machine embersEngine;
    
    public FileOrganizerApp()
    {
        embersEngine = new Machine();
        
        // Configure security - only allow safe file operations
        embersEngine.SetTypeAccessPolicy(new[]
        {
            "System.IO.File",
            "System.IO.Directory",
            "System.IO.Path",
            "System.IO.FileInfo",
            "System.Text.RegularExpressions.*"
        }, SecurityMode.WhitelistOnly);
        
        // Inject custom DSL functions
        embersEngine.InjectFromCallingAssembly();
        
        // Load user scripts from app data
        LoadUserScripts();
    }
    
    public void ExecuteUserRule(string ruleName)
    {
        embersEngine.ExecuteText($"run_rule('{ruleName}')");
    }
    
    private void LoadUserScripts()
    {
        string scriptsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileOrganizer", "Scripts"
        );
        
        if (Directory.Exists(scriptsPath))
        {
            foreach (var file in Directory.GetFiles(scriptsPath, "*.rb"))
            {
                try
                {
                    string script = File.ReadAllText(file);
                    embersEngine.ExecuteText(script);
                    Console.WriteLine($"Loaded rule: {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading {file}: {ex.Message}");
                }
            }
        }
    }
}

// DSL: Simple file matching and actions
[HostFunction("files_matching")]
internal class FilesMatchingFunction : HostFunction
{
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var pattern = values[0].ToString();
        var directory = values.Count > 1 ? values[1].ToString() : Directory.GetCurrentDirectory();
        
        var regex = new System.Text.RegularExpressions.Regex(pattern);
        return Directory.GetFiles(directory)
            .Where(f => regex.IsMatch(Path.GetFileName(f)))
            .ToList();
    }
}

[HostFunction("move_to")]
internal class MoveToFunction : HostFunction
{
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var filePath = values[0].ToString();
        var destination = values[1].ToString();
        
        Directory.CreateDirectory(destination);
        string fileName = Path.GetFileName(filePath);
        string destPath = Path.Combine(destination, fileName);
        
        File.Move(filePath, destPath, overwrite: true);
        return destPath;
    }
}

[HostFunction("older_than_days")]
internal class OlderThanDaysFunction : HostFunction
{
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var filePath = values[0].ToString();
        var days = Convert.ToInt32(values[1]);
        
        var fileInfo = new FileInfo(filePath);
        return (DateTime.Now - fileInfo.LastWriteTime).TotalDays > days;
    }
}

[HostFunction("file_size_mb")]
internal class FileSizeMbFunction : HostFunction
{
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var filePath = values[0].ToString();
        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length / (1024.0 * 1024.0);
    }
}
```

**User Script Example** (`organize_downloads.rb`):

```ruby
# User-created automation rule
def organize_downloads
  downloads = "C:/Users/#{System.Environment.UserName}/Downloads"
  
  # Organize images
  files_matching('.*\.(jpg|png|gif)$', downloads).each do |file|
    if older_than_days(file, 30)
      move_to(file, "#{downloads}/Archive/Images")
      puts "Archived old image: #{file}"
    else
      move_to(file, "#{downloads}/Images")
      puts "Organized image: #{file}"
    end
  end
  
  # Organize large files
  files_matching('.*', downloads).each do |file|
    if file_size_mb(file) > 100
      move_to(file, "#{downloads}/Large Files")
      puts "Moved large file: #{file}"
    end
  end
  
  # Organize by file type
  organize_by_extension(downloads, 'pdf', 'Documents/PDFs')
  organize_by_extension(downloads, 'zip', 'Archives')
  organize_by_extension(downloads, 'exe', 'Installers')
end

def organize_by_extension(base_dir, ext, target_folder)
  files_matching(".*\\.#{ext}$", base_dir).each do |file|
    move_to(file, "#{base_dir}/#{target_folder}")
  end
end

# Register this rule
def run_rule(name)
  organize_downloads if name == 'downloads'
end
```

**User-Facing Interface**:

```csharp
// In your WPF/WinForms/Avalonia UI
private void btnRunRule_Click(object sender, EventArgs e)
{
    var organizer = new FileOrganizerApp();
    organizer.ExecuteUserRule("downloads");
    MessageBox.Show("Files organized successfully!");
}

// Script editor for users
private void btnEditScript_Click(object sender, EventArgs e)
{
    string scriptPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FileOrganizer", "Scripts", "organize_downloads.rb"
    );
    
    // Open in built-in editor or system default
    Process.Start("notepad.exe", scriptPath);
}
```

---

### 3. Plugin System with Sandboxing

**Use Case**: Allow third-party developers to extend your application safely.

```csharp
public class PluginManager
{
    public void LoadPlugin(string pluginPath)
    {
        var machine = new Machine();
        
        // Strict sandboxing - only expose plugin API
        machine.SetTypeAccessPolicy(new[]
        {
            "MyApp.PluginAPI.*"
        }, SecurityMode.WhitelistOnly);
        
        // Isolated context per plugin
        machine.ExecuteText(File.ReadAllText(pluginPath));
    }
}
```