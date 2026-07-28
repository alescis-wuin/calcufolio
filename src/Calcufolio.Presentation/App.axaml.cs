using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Calcufolio.Presentation.ViewModels;
using Calcufolio.Presentation.Views;

namespace Calcufolio.Presentation;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainViewModel viewModel = new();

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };

            _ = viewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
