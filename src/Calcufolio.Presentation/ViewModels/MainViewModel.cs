using System.Collections.ObjectModel;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.Editor.Actions;
using Calcufolio.Application.Interaction.State;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calcufolio.Presentation.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly ICalculatorController _controller;
    private readonly ObservableCollection<CalculationHistoryEntryViewModel> _historyEntries = [];

    public MainViewModel(
        ICalculatorController controller,
        ICalculatorStateStore stateStore)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(stateStore);

        _controller = controller;

        HistoryEntries =
            new ReadOnlyObservableCollection<CalculationHistoryEntryViewModel>(
                _historyEntries);

        stateStore.StateChanged += OnStateChanged;

        ApplyState(stateStore.Current);
    }

    [ObservableProperty]
    public partial string Expression { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DisplayValue { get; set; } = "0";

    [ObservableProperty]
    public partial int EditorCaretIndex { get; set; } = 1;

    [ObservableProperty]
    public partial int EditorSelectionStart { get; set; } = 1;

    [ObservableProperty]
    public partial int EditorSelectionEnd { get; set; } = 1;

    public string AngleMode { get; } = "DEG";

    public string MemoryStatus { get; } = "M  0";

    public ReadOnlyObservableCollection<CalculationHistoryEntryViewModel> HistoryEntries { get; }

    [ObservableProperty]
    public partial bool IsStartupToastVisible { get; set; } = true;

    [RelayCommand]
    private void AppendDigit(
        string digit)
    {
        _controller.Dispatch(
            new AppendDigitAction(digit));
    }

    [RelayCommand]
    private void AppendDecimalSeparator()
    {
        _controller.Dispatch(
            new AppendDecimalSeparatorAction());
    }

    [RelayCommand]
    private void SelectOperator(
        string operatorSymbol)
    {
        _controller.Dispatch(
            new SelectOperatorAction(operatorSymbol));
    }

    [RelayCommand]
    private void Backspace()
    {
        _controller.Dispatch(
            new EditInputAction(
                new BackspaceEditorAction()));
    }

    [RelayCommand]
    private void Evaluate()
    {
        _controller.Dispatch(
            new EvaluateAction());
    }

    [RelayCommand]
    private void Clear()
    {
        _controller.Dispatch(
            new ClearAction());
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(
            TimeSpan.FromSeconds(4));

        IsStartupToastVisible = false;
    }

    private void OnStateChanged(
        object? _,
        CalculatorStateChangedEventArgs eventArgs)
    {
        ApplyState(eventArgs.State);
    }

    private void ApplyState(
        CalculatorState state)
    {
        Expression = state.Expression;
        DisplayValue = state.DisplayValue;

        EditorCaretIndex =
            state.Editor.CaretIndex;

        EditorSelectionStart =
            state.Editor.SelectionStart;

        EditorSelectionEnd =
            state.Editor.SelectionEnd;

        _historyEntries.Clear();

        foreach (CalculationHistoryEntry historyEntry in state.HistoryEntries)
        {
            _historyEntries.Add(
                new CalculationHistoryEntryViewModel(
                    historyEntry.Expression,
                    historyEntry.Result));
        }
    }
}
