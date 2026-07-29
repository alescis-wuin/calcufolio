namespace Calcufolio.Domain.Calculations;

public interface ICalculationEngine
{
    double Calculate(
        double leftOperand,
        BinaryOperator operation,
        double rightOperand);
}
