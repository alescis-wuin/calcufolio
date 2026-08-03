using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;
using Calcufolio.Presentation.ViewModels;
using Calcufolio.Presentation.Views;

namespace Calcufolio.Presentation;

public partial class App : global::Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ICalculationEngine calculationEngine =
                new CalculationEngine();

            ICalculatorStateStore stateStore =
                new CalculatorStateStore();

            ICalculatorController controller =
                new CalculatorController(
                    calculationEngine,
                    stateStore);

            MainViewModel viewModel =
                new(
                    controller,
                    stateStore);

            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };

            _ = viewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
