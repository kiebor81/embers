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

**Use Case**: Enable game designers to script behaviors, quest logic, and NPC interactions in Embers without recompiling the game.

#### Dynamic Node Interaction

Expose Godot nodes to Embers scripts for real-time manipulation:

```csharp
using Embers;
using Embers.Host;
using Godot;

public partial class EmbersGameScript : Node
{
    private Machine embers;
    
    public override void _Ready()
    {
        embers = new Machine();
        
        // Register host functions for node access
        embers.InjectFromCallingAssembly();
        
        // Pass Godot context to Embers
        embers.RootContext.SetLocalValue("scene", GetTree().Root);
        embers.RootContext.SetLocalValue("player", GetNode<CharacterBody2D>("Player"));
        
        // Load and execute quest script
        string questScript = FileAccess.Open("res://scripts/quest_01.rb", FileAccess.ModeFlags.Read).GetAsText();
        embers.Execute(questScript);
    }
}

// Host function for print to gd console
[HostFunction("gd_print")]
internal class GdPrintFunction : HostFunction
{
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        foreach (var value in values)
        {
            GD.Print(ToPrintable(value));
        }

        return null;
    }

    private static object ToPrintable(object value)
    {
        if (value == null)
            return "nil";

        if (value is Variant v)
            return v;

        return value.ToString() ?? "nil";
    }
}

// Host function for finding nodes
[HostFunction("find_node")]
internal class FindNodeFunction : HostFunction
{
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var nodePath = values[0].ToString();
        var scene = context.GetLocalValue("scene") as Node;
        return scene?.GetNode(nodePath);
    }
}

// Host function for spawning enemies
[HostFunction("spawn_enemy")]
internal class SpawnEnemyFunction : HostFunction
{
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        var enemyType = values[0].ToString();
        var x = Convert.ToDouble(values[1]);
        var y = Convert.ToDouble(values[2]);
        
        var scene = context.GetLocalValue("scene") as Node;
        var enemyScene = GD.Load<PackedScene>($"res://enemies/{enemyType}.tscn");
        var enemy = enemyScene.Instantiate<Node2D>();
        enemy.Position = new Vector2((float)x, (float)y);
        scene.AddChild(enemy);
        
        return enemy;
    }
}
```

**Embers Quest Script** (`quest_01.rb`):

```ruby
# Dynamic quest behavior
def on_quest_trigger
  gd_print("Quest started!")
  
  # Spawn enemies dynamically
  spawn_enemy("goblin", 100, 200)
  spawn_enemy("goblin", 150, 200)
  
  # Find and modify UI
  ui = find_node("UI/QuestPanel")
  ui.Visible = true
  ui.GetNode("Label").Text = "Defeat the goblins!"
end

# Enemy AI behavior
def enemy_think(enemy)
  player = $player
  distance = enemy.Position.DistanceTo(player.Position)
  
  if distance < 100
    # Chase player
    direction = (player.Position - enemy.Position).Normalized()
    enemy.Velocity = direction * 50
  else
    # Patrol
    enemy.Velocity = Godot::Vector2.new(0, 0)
  end
end
```

#### Autoload Integration

Create a Godot autoload singleton for global scripting:

```csharp
    [GlobalClass]
    public partial class Runtime : Node
    {
        private Machine Machine { get; set; }

        private System.Collections.Generic.Dictionary<string, string> LoadedScripts { get; set; } = [];

        private string ScriptContext => string.Join("\n", LoadedScripts.Values);

        public override void _Ready()
        {
            Machine = new Machine();
            Machine.InjectFromCallingAssembly();
            LoadGlobalScripts("res://autoloads/");
            GD.Print("Embers Engine initialized");
        }

        private void LoadGlobalScripts(string directory)
        {
            var dir = DirAccess.Open(directory);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();

                while (fileName != "")
                {
                    if (fileName.EndsWith(".rb"))
                    {
                        string script = FileAccess.Open($"{directory}{fileName}", FileAccess.ModeFlags.Read).GetAsText();
                        LoadedScripts[fileName.Replace(".rb", "")] = script;
                        GD.Print($"Loaded Embers autoload: {fileName}");
                    }
                    fileName = dir.GetNext();
                }
            }
        }

        public Variant Execute(string functionName, Array args)
        {
            var embersArgs = string.Join(", ", args.Select(ToEmbersLiteral));
            string call =
                $"{ScriptContext}\n{functionName}({embersArgs})";

            return ToGodotVariant(Machine.Execute(call));
        }

        private static string ToEmbersLiteral(Variant v)
        {
            switch (v.VariantType)
            {
                case Variant.Type.Nil:
                    return "nil";

                case Variant.Type.Bool:
                    return (bool)v ? "true" : "false";

                case Variant.Type.Int:
                    return ((long)v).ToString(CultureInfo.InvariantCulture);

                case Variant.Type.Float:
                    // Embers uses Ruby-like numeric literals; invariant culture avoids commas.
                    return ((double)v).ToString(CultureInfo.InvariantCulture);

                case Variant.Type.String:
                    return $"\"{EscapeRubyString((string)v)}\"";

                case Variant.Type.Array:
                    {
                        var arr = (Array)v;
                        return "[" + string.Join(", ", arr.Select(ToEmbersLiteral)) + "]";
                    }

                case Variant.Type.Dictionary:
                    {
                        var dict = (Dictionary)v;
                        // Ruby hash: { "k" => v, "k2" => v2 }
                        var pairs = dict.Select(kv => $"{ToEmbersLiteral(kv.Key)} => {ToEmbersLiteral(kv.Value)}");
                        return "{ " + string.Join(", ", pairs) + " }";
                    }

                default:
                    // Fallback: pass as string literal so Embers always parses
                    return $"\"{EscapeRubyString(v.ToString())}\"";
            }
        }

        private static string EscapeRubyString(string s) =>
            s.Replace("\\", "\\\\")
             .Replace("\"", "\\\"")
             .Replace("\n", "\\n")
             .Replace("\r", "\\r")
             .Replace("\t", "\\t");

        private static Variant ToGodotVariant(object? value)
        {
            if (value == null)
                return new Variant(); // Return default-constructed Variant for 'nil'

            return value switch
            {
                bool b => b,
                int i => i,
                long l => (double)l,
                float f => (double)f,
                double d => d,
                string s => s,

                // Godot types (if Embers ever returns them from a Host Function)
                GodotObject o => o,
                _ => value.ToString() ?? ""
            };
        }

    }
```

**Global Script** (`/autoloads/game_utils.rb`):

```ruby
# Global utility functions available throughout the game
def calculate_damage(attacker_level, defender_level, base_damage)
  multiplier = 1.0 + ((attacker_level - defender_level) * 0.1)
  (base_damage * multiplier).to_i.clamp(1, 9999)
end
```

Then call from any gdscript:

```gdscript
var damage = Embers.Execute("calculate_damage", [10, 5, 25])
print("Damage dealt: %s" % damage);
```

```c#
var embers_engine = GetNode<Runtime>("/root/Runtime");
var damage = embers_engine.Execute("calculate_damage", [10, 5, 25])
GD.Print($"Damage dealt: {damage}");
```

Consuming scripts from a `user://` location with functionality exposed only via `HostFunction` effectively creates a user facing scripting model that can be used for player content modification and creation.

---

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
  downloads = "C:/Users/#{System::Environment.UserName}/Downloads"
  
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