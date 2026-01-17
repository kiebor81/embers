using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Embers.ISE.ViewModels;
using Embers.ISE.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Embers.Security;

namespace Embers.ISE
{
    public partial class App : Application
    {
        private const string TabStateFileName = "ISETabs.json";
        private const string ConfigStateFileName = "ISEConfig.json";

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                var viewModel = new MainWindowViewModel();
                RestoreTabs(viewModel);
                RestoreConfiguration(viewModel);

                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel,
                };

                desktop.Exit += (_, _) =>
                {
                    SaveTabs(viewModel);
                    SaveConfiguration(viewModel);
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }

        private static void RestoreTabs(MainWindowViewModel viewModel)
        {
            var configPath = GetTabStatePath();
            if (!File.Exists(configPath))
                return;

            try
            {
                var json = File.ReadAllText(configPath);
                var paths = JsonSerializer.Deserialize<List<string>>(json);
                if (paths == null || paths.Count == 0)
                    return;

                viewModel.RestoreTabs(paths);
            }
            catch
            {
                // Ignore malformed config.
            }
        }

        private static void SaveTabs(MainWindowViewModel viewModel)
        {
            var configPath = GetTabStatePath();
            var directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var paths = viewModel.GetOpenFilePaths();
            var json = JsonSerializer.Serialize(paths, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(configPath, json);
        }

        private static void RestoreConfiguration(MainWindowViewModel viewModel)
        {
            var configPath = GetConfigStatePath();
            if (!File.Exists(configPath))
                return;

            try
            {
                var json = File.ReadAllText(configPath);
                var state = JsonSerializer.Deserialize<IseConfigState>(json);
                if (state == null)
                    return;

                viewModel.RestoreConfiguration(state.SecurityMode, state.WhitelistEntries, state.ReferenceAssemblies);
            }
            catch
            {
                // Ignore malformed config.
            }
        }

        private static void SaveConfiguration(MainWindowViewModel viewModel)
        {
            var configPath = GetConfigStatePath();
            var directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var state = new IseConfigState
            {
                SecurityMode = viewModel.SelectedSecurityMode,
                WhitelistEntries = viewModel.WhitelistEntries.ToList(),
                ReferenceAssemblies = viewModel.ReferenceAssemblies.ToList()
            };

            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(configPath, json);
        }

        private static string GetTabStatePath()
        {
            var root = Directory.GetCurrentDirectory();
            return Path.Combine(root, ".vs", TabStateFileName);
        }

        private static string GetConfigStatePath()
        {
            var root = Directory.GetCurrentDirectory();
            return Path.Combine(root, ".vs", ConfigStateFileName);
        }

        private sealed class IseConfigState
        {
            public List<string> ReferenceAssemblies { get; set; } = [];
            public List<string> WhitelistEntries { get; set; } = [];
            public SecurityMode SecurityMode { get; set; } = SecurityMode.Unrestricted;
        }
    }
}
