using System.Globalization;
using Calcufolio.Application.Calculations;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Interaction.Preview;

public sealed class CalculationPreviewService : ICalculationPreviewService
{
    private readonly ICalculationEngine _calculationEngine;

    public CalculationPreviewService(
        ICalculationEngine calculationEngine)
    {
        ArgumentNullException.ThrowIfNull(calculationEngine);

        _calculationEngine = calculationEngine;
    }

    public CalculationPreview? Create(
        CalculatorState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.HasError ||
            state.PendingOperation is null ||
            state.ReplaceDisplayOnNextInput)
        {
            return null;
        }

        PendingBinaryOperation pendingOperation =
            state.PendingOperation;

        if (!double.IsFinite(
                pendingOperation.LeftOperand) ||
            !TryParseDisplayValue(
                state.DisplayValue,
                out double rightOperand))
        {
            return null;
        }

        string? operatorSymbol =
            GetDisplaySymbol(
                pendingOperation.Operation);

        if (operatorSymbol is null)
        {
            return null;
        }

        try
        {
            double result =
                _calculationEngine.Calculate(
                    pendingOperation.LeftOperand,
                    pendingOperation.Operation,
                    rightOperand);

            string expression =
                $"{FormatNumber(pendingOperation.LeftOperand)} " +
                $"{operatorSymbol} " +
                $"{FormatNumber(rightOperand)} =";

            return new CalculationPreview(
                expression,
                FormatNumber(result));
        }
        catch (DivideByZeroException)
        {
            return null;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static bool TryParseDisplayValue(
        string displayValue,
        out double value)
    {
        return double.TryParse(
                displayValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value) &&
            double.IsFinite(value);
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

    private static string? GetDisplaySymbol(
        BinaryOperator operation)
    {
        return operation switch
        {
            BinaryOperator.Add => "+",
            BinaryOperator.Subtract => "−",
            BinaryOperator.Multiply => "×",
            BinaryOperator.Divide => "÷",
            _ => null,
        };
    }
}
