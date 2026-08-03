using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Calcufolio.Presentation.Input;
using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Views;

public partial class MainWindow : Window
{
    private readonly AvaloniaKeyboardInputRouter? _keyboardInputRouter;
    private readonly AvaloniaSelectionInputAdapter? _selectionInputAdapter;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(
        AvaloniaKeyboardInputRouter keyboardInputRouter,
        AvaloniaSelectionInputAdapter selectionInputAdapter)
    {
        ArgumentNullException.ThrowIfNull(keyboardInputRouter);
        ArgumentNullException.ThrowIfNull(selectionInputAdapter);

        _keyboardInputRouter = keyboardInputRouter;
        _selectionInputAdapter = selectionInputAdapter;

        InitializeComponent();

        AddHandler(
            InputElement.KeyDownEvent,
            OnKeyDown,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);

        AddHandler(
            InputElement.TextInputEvent,
            OnTextInput,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);

        CalculatorDisplay.AddHandler(
            InputElement.PointerReleasedEvent,
            OnDisplayPointerReleased,
            RoutingStrategies.Bubble,
            handledEventsToo: true);

        Opened += OnOpened;
    }

    private void OnOpened(
        object? sender,
        EventArgs eventArgs)
    {
        RestoreDisplayFocusAndSelection();
    }

    private void OnKeyDown(
        object? sender,
        KeyEventArgs eventArgs)
    {
        if (_keyboardInputRouter?.RouteKey(
                eventArgs.Key,
                eventArgs.KeyModifiers) == true)
        {
            eventArgs.Handled = true;
            RestoreDisplayFocusAndSelection();
        }
    }

    private void OnTextInput(
        object? sender,
        TextInputEventArgs eventArgs)
    {
        if (_keyboardInputRouter?.RouteText(
                eventArgs.Text) == true)
        {
            eventArgs.Handled = true;
            RestoreDisplayFocusAndSelection();
        }
    }

    private void OnDisplayPointerReleased(
        object? sender,
        PointerReleasedEventArgs eventArgs)
    {
        _selectionInputAdapter?.Synchronize(
            CalculatorDisplay);

        RestoreDisplayFocusAndSelection();
    }

    private void RestoreDisplayFocusAndSelection()
    {
        Dispatcher.UIThread.Post(
            () =>
            {
                if (DataContext is MainViewModel viewModel)
                {
                    CalculatorDisplay.SelectionStart =
                        viewModel.EditorSelectionStart;

                    CalculatorDisplay.SelectionEnd =
                        viewModel.EditorSelectionEnd;

                    CalculatorDisplay.CaretIndex =
                        viewModel.EditorCaretIndex;
                }

                CalculatorDisplay.Focus();
            },
            DispatcherPriority.Input);
    }
}
