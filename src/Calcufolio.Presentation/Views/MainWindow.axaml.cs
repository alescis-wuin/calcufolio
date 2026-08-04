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
    private int _displaySynchronizationVersion;

    public MainWindow()
    {
        InitializeComponent();
        ConfigureDisplay();
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
        ConfigureDisplay();

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

    private void ConfigureDisplay()
    {
        CalculatorDisplay.CaretBlinkInterval =
            TimeSpan.Zero;

        CalculatorDisplay.TextChanged +=
            OnDisplayTextChanged;
    }

    private void OnOpened(
        object? sender,
        EventArgs eventArgs)
    {
        QueueDisplaySynchronization();
    }

    private async void OnKeyDown(
        object? sender,
        KeyEventArgs eventArgs)
    {
        if (_keyboardInputRouter is null ||
            !AvaloniaKeyboardInputRouter.CanRouteKey(
                eventArgs.Key,
                eventArgs.KeyModifiers))
        {
            return;
        }

        eventArgs.Handled = true;

        await _keyboardInputRouter.RouteKeyAsync(
            eventArgs.Key,
            eventArgs.KeyModifiers);

        QueueDisplaySynchronization();
    }

    private void OnTextInput(
        object? sender,
        TextInputEventArgs eventArgs)
    {
        if (_keyboardInputRouter?.RouteText(
                eventArgs.Text) == true)
        {
            eventArgs.Handled = true;
            QueueDisplaySynchronization();
        }
    }

    private void OnDisplayPointerReleased(
        object? sender,
        PointerReleasedEventArgs eventArgs)
    {
        _selectionInputAdapter?.Synchronize(
            CalculatorDisplay);

        QueueDisplaySynchronization();
    }

    private void OnDisplayTextChanged(
        object? sender,
        TextChangedEventArgs eventArgs)
    {
        QueueDisplaySynchronization();
    }

    private void QueueDisplaySynchronization()
    {
        int synchronizationVersion =
            ++_displaySynchronizationVersion;

        Dispatcher.UIThread.Post(
            () =>
            {
                if (synchronizationVersion !=
                    _displaySynchronizationVersion)
                {
                    return;
                }

                ApplyDisplaySynchronization();
            },
            DispatcherPriority.Render);
    }

    private void ApplyDisplaySynchronization()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            CalculatorDisplay.Focus();
            return;
        }

        int textLength =
            CalculatorDisplay.Text?.Length ?? 0;

        int caretIndex = Math.Clamp(
            viewModel.EditorCaretIndex,
            0,
            textLength);

        int selectionStart = Math.Clamp(
            viewModel.EditorSelectionStart,
            0,
            textLength);

        int selectionEnd = Math.Clamp(
            viewModel.EditorSelectionEnd,
            0,
            textLength);

        CalculatorDisplay.Focus();

        CalculatorDisplay.CaretIndex =
            caretIndex;

        CalculatorDisplay.SelectionStart =
            selectionStart;

        CalculatorDisplay.SelectionEnd =
            selectionEnd;
    }
}
