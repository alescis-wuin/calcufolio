using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.Editor.Reducer;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;
using Calcufolio.Presentation.Input;
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

            IEditorStateReducer editorStateReducer =
                new EditorStateReducer();

            ICalculatorStateStore stateStore =
                new CalculatorStateStore();

            ICalculatorController controller =
                new CalculatorController(
                    calculationEngine,
                    editorStateReducer,
                    stateStore);

            MainViewModel viewModel =
                new(
                    controller,
                    stateStore);

            AvaloniaKeyboardInputRouter keyboardInputRouter =
                new(controller);

            AvaloniaSelectionInputAdapter selectionInputAdapter =
                new(controller);

            desktop.MainWindow = new MainWindow(
                keyboardInputRouter,
                selectionInputAdapter)
            {
                DataContext = viewModel,
            };

            _ = viewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
