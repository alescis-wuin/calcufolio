using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Calculations;

public interface ICalculatorSession
{
    PendingBinaryOperation? PendingOperation { get; }

    void SelectOperation(
        double leftOperand,
        BinaryOperator operation);

    double Evaluate(double rightOperand);

    void Clear();
}
