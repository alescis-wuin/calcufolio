using System.Globalization;
using Calcufolio.Application.Calculations;
using Calcufolio.Application.Expressions;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Interaction.Preview;

public sealed class CalculationPreviewService : ICalculationPreviewService
{
    private readonly IExpressionEvaluationService _expressionEvaluationService;

    public CalculationPreviewService(
        IExpressionEvaluationService expressionEvaluationService)
    {
        ArgumentNullException.ThrowIfNull(expressionEvaluationService);

        _expressionEvaluationService = expressionEvaluationService;
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
                pendingOperation.LeftOperand))
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

        string evaluationExpression =
            $"{FormatEvaluationNumber(pendingOperation.LeftOperand)} " +
            $"{operatorSymbol} {state.DisplayValue}";

        ExpressionEvaluationResult result =
            _expressionEvaluationService.Evaluate(
                evaluationExpression);

        if (!result.IsSuccess ||
            result.DisplayValue is not string displayValue)
        {
            return null;
        }

        string displayExpression =
            $"{FormatNumber(pendingOperation.LeftOperand)} " +
            $"{operatorSymbol} {state.DisplayValue} =";

        return new CalculationPreview(
            displayExpression,
            displayValue);
    }

    private static string FormatEvaluationNumber(
        double value)
    {
        if (value == 0.0)
        {
            return "0";
        }

        return value.ToString(
            "R",
            CultureInfo.InvariantCulture);
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
