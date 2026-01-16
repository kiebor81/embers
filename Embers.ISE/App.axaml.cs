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

namespace Embers.ISE
{
    public partial class App : Application
    {
        private const string TabStateFileName = "ISETabs.json";

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

                desktop.MainWindow = new MainWindow
                {
                    DataContext = viewModel,
                };

                desktop.Exit += (_, _) => SaveTabs(viewModel);
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

        private static string GetTabStatePath()
        {
            var root = Directory.GetCurrentDirectory();
            return Path.Combine(root, ".vs", TabStateFileName);
        }
    }
}
