using System.Globalization;
using Calcufolio.Domain.Calculations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calcufolio.Presentation.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private double? _leftOperand;
    private BinaryOperator? _pendingOperator;
    private bool _shouldReplaceDisplay;

    [ObservableProperty]
    public partial string Expression { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DisplayValue { get; set; } = "0";

    public string AngleMode { get; } = "DEG";

    public string MemoryStatus { get; } = "M  0";

    public IReadOnlyList<CalculationHistoryEntryViewModel> HistoryEntries { get; } = [];

    [ObservableProperty]
    public partial bool IsStartupToastVisible { get; set; } = true;

    [RelayCommand]
    private void AppendDigit(string digit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(digit);

        if (digit.Length != 1 ||
            !char.IsAsciiDigit(digit[0]))
        {
            throw new ArgumentException(
                "The digit must contain exactly one ASCII numeric character.",
                nameof(digit));
        }

        if (_shouldReplaceDisplay ||
            DisplayValue == "0")
        {
            DisplayValue = digit;
            _shouldReplaceDisplay = false;
            return;
        }

        DisplayValue = $"{DisplayValue}{digit}";
    }

    [RelayCommand]
    private void AppendDecimalSeparator()
    {
        if (_shouldReplaceDisplay)
        {
            DisplayValue = "0.";
            _shouldReplaceDisplay = false;
            return;
        }

        if (!DisplayValue.Contains(
                '.',
                StringComparison.Ordinal))
        {
            DisplayValue = $"{DisplayValue}.";
        }
    }

    [RelayCommand]
    private void SelectOperator(string operatorSymbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorSymbol);

        BinaryOperator operation = ParseOperator(operatorSymbol);

        if (!_shouldReplaceDisplay ||
            _leftOperand is null)
        {
            _leftOperand = ParseDisplayValue();
        }

        _pendingOperator = operation;
        Expression = CreateExpression();
        _shouldReplaceDisplay = true;
    }

    [RelayCommand]
    private void Clear()
    {
        _leftOperand = null;
        _pendingOperator = null;
        _shouldReplaceDisplay = false;
        Expression = string.Empty;
        DisplayValue = "0";
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(4));

        IsStartupToastVisible = false;
    }

    private double ParseDisplayValue()
    {
        if (!double.TryParse(
                DisplayValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double operand) ||
            !double.IsFinite(operand))
        {
            throw new InvalidOperationException(
                "The display does not contain a valid finite operand.");
        }

        return operand;
    }

    private string CreateExpression()
    {
        if (_leftOperand is null ||
            _pendingOperator is null)
        {
            return string.Empty;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{_leftOperand.Value} {ToDisplaySymbol(_pendingOperator.Value)}");
    }

    private static BinaryOperator ParseOperator(string operatorSymbol)
    {
        return operatorSymbol switch
        {
            "+" => BinaryOperator.Add,
            "−" => BinaryOperator.Subtract,
            "×" => BinaryOperator.Multiply,
            "÷" => BinaryOperator.Divide,
            _ => throw new ArgumentException(
                "The operator symbol is not supported.",
                nameof(operatorSymbol)),
        };
    }

    private static string ToDisplaySymbol(BinaryOperator operation)
    {
        return operation switch
        {
            BinaryOperator.Add => "+",
            BinaryOperator.Subtract => "−",
            BinaryOperator.Multiply => "×",
            BinaryOperator.Divide => "÷",
            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "The binary operator is not supported."),
        };
    }
}
