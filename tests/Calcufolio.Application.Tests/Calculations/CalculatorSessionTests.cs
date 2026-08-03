using Calcufolio.Application.Calculations;
using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Tests.Calculations;

public sealed class CalculatorSessionTests
{
    [Fact]
    public void InitialStateHasNoPendingOperation()
    {
        CalculatorSession session = CreateSession();

        Assert.Null(session.PendingOperation);
    }

    [Fact]
    public void SelectOperationStoresPendingOperation()
    {
        CalculatorSession session = CreateSession();

        session.SelectOperation(
            12.0,
            BinaryOperator.Add);

        PendingBinaryOperation pendingOperation =
            Assert.IsType<PendingBinaryOperation>(
                session.PendingOperation);

        Assert.Equal(
            12.0,
            pendingOperation.LeftOperand);

        Assert.Equal(
            BinaryOperator.Add,
            pendingOperation.Operation);
    }

    [Fact]
    public void SelectOperationReplacesPendingOperation()
    {
        CalculatorSession session = CreateSession();

        session.SelectOperation(
            12.0,
            BinaryOperator.Add);

        session.SelectOperation(
            7.0,
            BinaryOperator.Multiply);

        PendingBinaryOperation pendingOperation =
            Assert.IsType<PendingBinaryOperation>(
                session.PendingOperation);

        Assert.Equal(
            7.0,
            pendingOperation.LeftOperand);

        Assert.Equal(
            BinaryOperator.Multiply,
            pendingOperation.Operation);
    }

    [Fact]
    public void EvaluateReturnsResultAndClearsPendingOperation()
    {
        CalculatorSession session = CreateSession();

        session.SelectOperation(
            12.0,
            BinaryOperator.Add);

        double result = session.Evaluate(7.0);

        Assert.Equal(
            19.0,
            result);

        Assert.Null(session.PendingOperation);
    }

    [Fact]
    public void EvaluateWithoutPendingOperationThrows()
    {
        CalculatorSession session = CreateSession();

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => session.Evaluate(7.0));

        Assert.Equal(
            "No binary operation is pending.",
            exception.Message);
    }

    [Fact]
    public void ClearRemovesPendingOperation()
    {
        CalculatorSession session = CreateSession();

        session.SelectOperation(
            12.0,
            BinaryOperator.Subtract);

        session.Clear();

        Assert.Null(session.PendingOperation);
    }

    [Fact]
    public void FailedEvaluationPreservesPendingOperation()
    {
        CalculatorSession session = CreateSession();

        session.SelectOperation(
            12.0,
            BinaryOperator.Divide);

        Assert.Throws<DivideByZeroException>(
            () => session.Evaluate(0.0));

        PendingBinaryOperation pendingOperation =
            Assert.IsType<PendingBinaryOperation>(
                session.PendingOperation);

        Assert.Equal(
            12.0,
            pendingOperation.LeftOperand);

        Assert.Equal(
            BinaryOperator.Divide,
            pendingOperation.Operation);
    }

    [Fact]
    public void ConstructorRejectsMissingCalculationEngine()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new CalculatorSession(null!));

        Assert.Equal(
            "calculationEngine",
            exception.ParamName);
    }

    private static CalculatorSession CreateSession()
    {
        return new CalculatorSession(
            new CalculationEngine());
    }
}
