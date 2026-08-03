using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Calculations;

public sealed class CalculatorSession : ICalculatorSession
{
    private readonly ICalculationEngine _calculationEngine;

    public CalculatorSession(
        ICalculationEngine calculationEngine)
    {
        ArgumentNullException.ThrowIfNull(
            calculationEngine);

        _calculationEngine = calculationEngine;
    }

    public PendingBinaryOperation? PendingOperation
    {
        get;
        private set;
    }

    public void SelectOperation(
        double leftOperand,
        BinaryOperator operation)
    {
        PendingOperation = new PendingBinaryOperation(
            leftOperand,
            operation);
    }

    public double Evaluate(double rightOperand)
    {
        PendingBinaryOperation pendingOperation =
            PendingOperation ??
            throw new InvalidOperationException(
                "No binary operation is pending.");

        double result = _calculationEngine.Calculate(
            pendingOperation.LeftOperand,
            pendingOperation.Operation,
            rightOperand);

        PendingOperation = null;

        return result;
    }

    public void Clear()
    {
        PendingOperation = null;
    }
}
