using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Calcufolio.Application.Interaction.Clipboard;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.Editor.Reducer;
using Calcufolio.Application.Interaction.Preview;
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

            ICalculationPreviewService calculationPreviewService =
                new CalculationPreviewService(
                    calculationEngine);

            ICalculatorController controller =
                new CalculatorController(
                    calculationEngine,
                    editorStateReducer,
                    stateStore);

            IClipboardPort clipboardPort =
                new AvaloniaClipboardPort(
                    () => desktop.MainWindow?.Clipboard);

            IClipboardTextSanitizer clipboardTextSanitizer =
                new NumericClipboardTextSanitizer();

            ICalculatorClipboardController clipboardController =
                new CalculatorClipboardController(
                    controller,
                    stateStore,
                    clipboardPort,
                    clipboardTextSanitizer);

            MainViewModel viewModel =
                new(
                    controller,
                    stateStore,
                    calculationPreviewService);

            AvaloniaKeyboardInputRouter keyboardInputRouter =
                new(
                    controller,
                    clipboardController);

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
