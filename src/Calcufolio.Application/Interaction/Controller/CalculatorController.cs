using System.Collections.ObjectModel;
using System.Globalization;
using Calcufolio.Application.Calculations;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Interaction.Controller;

public sealed class CalculatorController : ICalculatorController
{
    private const int MaximumHistoryEntries = 20;

    private readonly ICalculationEngine _calculationEngine;
    private readonly ICalculatorStateStore _stateStore;

    public CalculatorController(
        ICalculationEngine calculationEngine,
        ICalculatorStateStore stateStore)
    {
        ArgumentNullException.ThrowIfNull(calculationEngine);
        ArgumentNullException.ThrowIfNull(stateStore);

        _calculationEngine = calculationEngine;
        _stateStore = stateStore;
    }

    public void Dispatch(
        CalculatorAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        switch (action)
        {
            case AppendDigitAction appendDigit:
                AppendDigit(appendDigit.Digit);
                break;

            case AppendDecimalSeparatorAction:
                AppendDecimalSeparator();
                break;

            case SelectOperatorAction selectOperator:
                SelectOperator(selectOperator.OperatorSymbol);
                break;

            case EvaluateAction:
                Evaluate();
                break;

            case ClearAction:
                Clear();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(action),
                    action,
                    "The calculator action is not supported.");
        }
    }

    private void AppendDigit(
        string digit)
    {
        CalculatorState state =
            PrepareForValueInput(_stateStore.Current);

        string displayValue = state.DisplayValue == "0"
            ? digit
            : $"{state.DisplayValue}{digit}";

        _stateStore.Replace(
            state with
            {
                DisplayValue = displayValue,
            });
    }

    private void AppendDecimalSeparator()
    {
        CalculatorState state =
            PrepareForValueInput(_stateStore.Current);

        if (state.DisplayValue.Contains(
                '.',
                StringComparison.Ordinal))
        {
            _stateStore.Replace(state);
            return;
        }

        _stateStore.Replace(
            state with
            {
                DisplayValue = $"{state.DisplayValue}.",
            });
    }

    private void SelectOperator(
        string operatorSymbol)
    {
        CalculatorState state = _stateStore.Current;

        if (state.HasError)
        {
            return;
        }

        BinaryOperator operation =
            ParseOperator(operatorSymbol);

        if (state.PendingOperation is not null &&
            !state.ReplaceDisplayOnNextInput)
        {
            state = EvaluatePendingOperation(state);

            if (state.HasError)
            {
                _stateStore.Replace(state);
                return;
            }
        }

        double leftOperand =
            state.PendingOperation?.LeftOperand ??
            ParseDisplayValue(state.DisplayValue);

        PendingBinaryOperation pendingOperation = new(
            leftOperand,
            operation);

        _stateStore.Replace(
            state with
            {
                Expression =
                    CreatePendingExpression(pendingOperation),
                PendingOperation = pendingOperation,
                ReplaceDisplayOnNextInput = true,
            });
    }

    private void Evaluate()
    {
        CalculatorState state = _stateStore.Current;

        if (state.HasError ||
            state.PendingOperation is null ||
            state.ReplaceDisplayOnNextInput)
        {
            return;
        }

        _stateStore.Replace(
            EvaluatePendingOperation(state));
    }

    private void Clear()
    {
        _stateStore.Replace(
            ResetCalculatorState(_stateStore.Current));
    }

    private CalculatorState EvaluatePendingOperation(
        CalculatorState state)
    {
        PendingBinaryOperation pendingOperation =
            state.PendingOperation ??
            throw new InvalidOperationException(
                "No binary operation is pending.");

        double rightOperand =
            ParseDisplayValue(state.DisplayValue);

        string completedExpression =
            CreateCompletedExpression(
                pendingOperation,
                rightOperand);

        try
        {
            double result = _calculationEngine.Calculate(
                pendingOperation.LeftOperand,
                pendingOperation.Operation,
                rightOperand);

            string displayValue =
                FormatNumber(result);

            return state with
            {
                DisplayValue = displayValue,
                Expression = completedExpression,
                PendingOperation = null,
                HasError = false,
                ReplaceDisplayOnNextInput = true,
                HistoryEntries = AddHistory(
                    state.HistoryEntries,
                    completedExpression,
                    displayValue),
            };
        }
        catch (DivideByZeroException exception)
        {
            return ShowError(
                state,
                exception.Message);
        }
        catch (OverflowException exception)
        {
            return ShowError(
                state,
                exception.Message);
        }
    }

    private static CalculatorState PrepareForValueInput(
        CalculatorState state)
    {
        if (state.HasError)
        {
            state = ResetCalculatorState(state);
        }

        if (!state.ReplaceDisplayOnNextInput)
        {
            return state;
        }

        string expression = state.PendingOperation is null
            ? string.Empty
            : state.Expression;

        return state with
        {
            Expression = expression,
            DisplayValue = "0",
            ReplaceDisplayOnNextInput = false,
        };
    }

    private static CalculatorState ResetCalculatorState(
        CalculatorState state)
    {
        return CalculatorState.Initial with
        {
            HistoryEntries = state.HistoryEntries,
        };
    }

    private static CalculatorState ShowError(
        CalculatorState state,
        string message)
    {
        return state with
        {
            Expression = message,
            DisplayValue = "Error",
            PendingOperation = null,
            HasError = true,
            ReplaceDisplayOnNextInput = true,
        };
    }

    private static ReadOnlyCollection<CalculationHistoryEntry> AddHistory(
        IReadOnlyList<CalculationHistoryEntry> currentEntries,
        string expression,
        string result)
    {
        List<CalculationHistoryEntry> entries =
        [
            new CalculationHistoryEntry(
                expression,
                result),
        ];

        entries.AddRange(
            currentEntries.Take(
                MaximumHistoryEntries - 1));

        return entries.AsReadOnly();
    }

    private static double ParseDisplayValue(
        string displayValue)
    {
        if (!double.TryParse(
                displayValue,
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

    private static string FormatNumber(
        double value)
    {
        if (value == 0.0)
        {
            return "0";
        }

        return value.ToString(
            "G15",
            CultureInfo.InvariantCulture);
    }

    private static BinaryOperator ParseOperator(
        string operatorSymbol)
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

    private static string ToDisplaySymbol(
        BinaryOperator operation)
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
