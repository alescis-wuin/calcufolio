namespace Calcufolio.Domain.Calculations;

public sealed class CalculationEngine : ICalculationEngine
{
    public double Calculate(
        double leftOperand,
        BinaryOperator operation,
        double rightOperand)
    {
        ValidateFiniteOperand(
            leftOperand,
            nameof(leftOperand));

        ValidateFiniteOperand(
            rightOperand,
            nameof(rightOperand));

        double result = operation switch
        {
            BinaryOperator.Add =>
                leftOperand + rightOperand,

            BinaryOperator.Subtract =>
                leftOperand - rightOperand,

            BinaryOperator.Multiply =>
                leftOperand * rightOperand,

            BinaryOperator.Divide =>
                Divide(leftOperand, rightOperand),

            _ => throw new ArgumentOutOfRangeException(
                nameof(operation),
                operation,
                "The binary operator is not supported."),
        };

        if (!double.IsFinite(result))
        {
            throw new OverflowException(
                "The calculation result is outside the supported numeric range.");
        }

        return result;
    }

    private static double Divide(
        double dividend,
        double divisor)
    {
        if (divisor == 0.0)
        {
            throw new DivideByZeroException(
                "Division by zero is not allowed.");
        }

        return dividend / divisor;
    }

    private static void ValidateFiniteOperand(
        double operand,
        string parameterName)
    {
        if (!double.IsFinite(operand))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                operand,
                "The operand must be a finite number.");
        }
    }
}
