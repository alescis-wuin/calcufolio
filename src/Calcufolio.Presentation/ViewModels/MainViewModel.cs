using System.Collections.ObjectModel;
using System.Globalization;
using Calcufolio.Application.Calculations;
using Calcufolio.Domain.Calculations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calcufolio.Presentation.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private const int MaximumHistoryEntries = 20;

    private readonly ICalculatorSession _calculatorSession;
    private readonly ObservableCollection<CalculationHistoryEntryViewModel> _historyEntries = [];
    private bool _hasError;
    private bool _shouldReplaceDisplay;

    public MainViewModel(
        ICalculatorSession calculatorSession)
    {
        ArgumentNullException.ThrowIfNull(
            calculatorSession);

        _calculatorSession = calculatorSession;
        HistoryEntries =
            new ReadOnlyObservableCollection<CalculationHistoryEntryViewModel>(
                _historyEntries);
    }

    [ObservableProperty]
    public partial string Expression { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DisplayValue { get; set; } = "0";

    public string AngleMode { get; } = "DEG";

    public string MemoryStatus { get; } = "M  0";

    public ReadOnlyObservableCollection<CalculationHistoryEntryViewModel> HistoryEntries { get; }

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

        PrepareForValueInput();

        DisplayValue = DisplayValue == "0"
            ? digit
            : $"{DisplayValue}{digit}";
    }

    [RelayCommand]
    private void AppendDecimalSeparator()
    {
        PrepareForValueInput();

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

        if (_hasError)
        {
            return;
        }

        BinaryOperator operation = ParseOperator(operatorSymbol);

        if (_calculatorSession.PendingOperation is not null &&
            !_shouldReplaceDisplay)
        {
            if (!TryEvaluatePendingOperation())
            {
                return;
            }
        }

        double leftOperand =
            _calculatorSession.PendingOperation?.LeftOperand ??
            ParseDisplayValue();

        _calculatorSession.SelectOperation(
            leftOperand,
            operation);

        Expression = CreatePendingExpression(
            _calculatorSession.PendingOperation!);

        _shouldReplaceDisplay = true;
    }

    [RelayCommand]
    private void Evaluate()
    {
        if (_hasError ||
            _calculatorSession.PendingOperation is null ||
            _shouldReplaceDisplay)
        {
            return;
        }

        _ = TryEvaluatePendingOperation();
    }

    [RelayCommand]
    private void Clear()
    {
        ResetCalculatorState();
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(4));

        IsStartupToastVisible = false;
    }

    private void PrepareForValueInput()
    {
        if (_hasError)
        {
            ResetCalculatorState();
        }

        if (!_shouldReplaceDisplay)
        {
            return;
        }

        if (_calculatorSession.PendingOperation is null)
        {
            Expression = string.Empty;
        }

        DisplayValue = "0";
        _shouldReplaceDisplay = false;
    }

    private bool TryEvaluatePendingOperation()
    {
        PendingBinaryOperation? pendingOperation =
            _calculatorSession.PendingOperation;

        if (pendingOperation is null)
        {
            return false;
        }

        double rightOperand = ParseDisplayValue();
        string completedExpression = CreateCompletedExpression(
            pendingOperation,
            rightOperand);

        try
        {
            double result = _calculatorSession.Evaluate(
                rightOperand);

            DisplayValue = FormatNumber(result);
            Expression = completedExpression;
            AddHistory(
                completedExpression,
                DisplayValue);

            _shouldReplaceDisplay = true;

            return true;
        }
        catch (DivideByZeroException exception)
        {
            ShowError(exception.Message);

            return false;
        }
        catch (OverflowException exception)
        {
            ShowError(exception.Message);

            return false;
        }
    }

    private void AddHistory(
        string expression,
        string result)
    {
        _historyEntries.Insert(
            0,
            new CalculationHistoryEntryViewModel(
                expression,
                result));

        if (_historyEntries.Count > MaximumHistoryEntries)
        {
            _historyEntries.RemoveAt(
                _historyEntries.Count - 1);
        }
    }

    private void ShowError(string message)
    {
        _calculatorSession.Clear();
        _hasError = true;
        _shouldReplaceDisplay = true;
        Expression = message;
        DisplayValue = "Error";
    }

    private void ResetCalculatorState()
    {
        _calculatorSession.Clear();
        _hasError = false;
        _shouldReplaceDisplay = false;
        Expression = string.Empty;
        DisplayValue = "0";
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

    private static string CreatePendingExpression(
        PendingBinaryOperation pendingOperation)
    {
        return $"{FormatNumber(pendingOperation.LeftOperand)} " +
            $"{ToDisplaySymbol(pendingOperation.Operation)}";
    }

    private static string CreateCompletedExpression(
        PendingBinaryOperation pendingOperation,
        double rightOperand)
    {
        return $"{FormatNumber(pendingOperation.LeftOperand)} " +
            $"{ToDisplaySymbol(pendingOperation.Operation)} " +
            $"{FormatNumber(rightOperand)} =";
    }

    private static string FormatNumber(double value)
    {
        if (value == 0.0)
        {
            return "0";
        }

        return value.ToString(
            "G15",
            CultureInfo.InvariantCulture);
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
