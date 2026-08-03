using Calcufolio.Domain.Calculations;

namespace Calcufolio.Domain.Tests.Calculations;

public sealed class CalculationEngineTests
{
    private readonly CalculationEngine _engine = new();

    [Theory]
    [InlineData(12.5, BinaryOperator.Add, 7.5, 20.0)]
    [InlineData(12.5, BinaryOperator.Subtract, 7.5, 5.0)]
    [InlineData(12.5, BinaryOperator.Multiply, 2.0, 25.0)]
    [InlineData(12.5, BinaryOperator.Divide, 2.0, 6.25)]
    public void CalculateReturnsExpectedResult(
        double leftOperand,
        BinaryOperator operation,
        double rightOperand,
        double expected)
    {
        double result = _engine.Calculate(
            leftOperand,
            operation,
            rightOperand);

        Assert.Equal(expected, result, 10);
    }

    [Fact]
    public void CalculateThrowsWhenDividingByPositiveZero()
    {
        AssertDivisionByZero(0.0);
    }

    [Fact]
    public void CalculateThrowsWhenDividingByNegativeZero()
    {
        AssertDivisionByZero(-0.0);
    }

    [Fact]
    public void CalculateThrowsWhenLeftOperandIsNotFinite()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _engine.Calculate(
                    double.PositiveInfinity,
                    BinaryOperator.Add,
                    1.0));

        Assert.Equal(
            "leftOperand",
            exception.ParamName);
    }

    [Fact]
    public void CalculateThrowsWhenRightOperandIsNotFinite()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _engine.Calculate(
                    1.0,
                    BinaryOperator.Add,
                    double.NaN));

        Assert.Equal(
            "rightOperand",
            exception.ParamName);
    }

    [Fact]
    public void CalculateThrowsWhenResultIsNotFinite()
    {
        OverflowException exception =
            Assert.Throws<OverflowException>(
                () => _engine.Calculate(
                    double.MaxValue,
                    BinaryOperator.Multiply,
                    2.0));

        Assert.Equal(
            "The calculation result is outside the supported numeric range.",
            exception.Message);
    }

    [Fact]
    public void CalculateThrowsForUnsupportedOperator()
    {
        BinaryOperator unsupportedOperator =
            (BinaryOperator)int.MaxValue;

        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _engine.Calculate(
                    1.0,
                    unsupportedOperator,
                    2.0));

        Assert.Equal(
            "operation",
            exception.ParamName);
    }

    private void AssertDivisionByZero(double rightOperand)
    {
        DivideByZeroException exception =
            Assert.Throws<DivideByZeroException>(
                () => _engine.Calculate(
                    10.0,
                    BinaryOperator.Divide,
                    rightOperand));

        Assert.Equal(
            "Division by zero is not allowed.",
            exception.Message);
    }
}
