using Calcufolio.Application.Calculations;
using Calcufolio.Application.Interaction.Editor.State;
using Calcufolio.Application.Interaction.Preview;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Tests.Interaction.Preview;

public sealed class CalculationPreviewServiceTests
{
    [Theory]
    [InlineData(
        12.0,
        BinaryOperator.Add,
        "7",
        "12 + 7 =",
        "19")]
    [InlineData(
        12.0,
        BinaryOperator.Subtract,
        "7",
        "12 − 7 =",
        "5")]
    [InlineData(
        12.0,
        BinaryOperator.Multiply,
        "7",
        "12 × 7 =",
        "84")]
    [InlineData(
        12.0,
        BinaryOperator.Divide,
        "4",
        "12 ÷ 4 =",
        "3")]
    public void CreateReturnsPreviewForSupportedOperation(
        double leftOperand,
        BinaryOperator operation,
        string rightOperand,
        string expectedExpression,
        string expectedResult)
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                leftOperand,
                operation,
                rightOperand);

        CalculationPreview preview =
            Assert.IsType<CalculationPreview>(
                service.Create(state));

        Assert.Equal(
            expectedExpression,
            preview.Expression);

        Assert.Equal(
            expectedResult,
            preview.Result);

        Assert.Equal(
            $"{expectedExpression} {expectedResult}",
            preview.DisplayText);
    }

    [Fact]
    public void CreateReturnsNullWithoutPendingOperation()
    {
        CalculationPreviewService service =
            CreateService();

        Assert.Null(
            service.Create(
                CalculatorState.Initial));
    }

    [Fact]
    public void CreateReturnsNullBeforeRightOperandInputStarts()
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                70.0,
                BinaryOperator.Multiply,
                "70") with
            {
                ReplaceDisplayOnNextInput = true,
            };

        Assert.Null(
            service.Create(state));
    }

    [Fact]
    public void CreateReturnsNullForErrorState()
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                70.0,
                BinaryOperator.Multiply,
                "279") with
            {
                HasError = true,
            };

        Assert.Null(
            service.Create(state));
    }

    [Theory]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("not-a-number")]
    [InlineData("Infinity")]
    public void CreateReturnsNullForInvalidRightOperand(
        string rightOperand)
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                70.0,
                BinaryOperator.Multiply,
                rightOperand);

        Assert.Null(
            service.Create(state));
    }

    [Fact]
    public void CreateReturnsNullForNonFiniteLeftOperand()
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                double.PositiveInfinity,
                BinaryOperator.Add,
                "1");

        Assert.Null(
            service.Create(state));
    }

    [Fact]
    public void CreateReturnsNullForUnsupportedOperation()
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                1.0,
                (BinaryOperator)999,
                "2");

        Assert.Null(
            service.Create(state));
    }

    [Fact]
    public void CreateSuppressesDivisionByZero()
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                12.0,
                BinaryOperator.Divide,
                "0");

        Assert.Null(
            service.Create(state));
    }

    [Fact]
    public void CreateSuppressesOverflow()
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                double.MaxValue,
                BinaryOperator.Multiply,
                "2");

        Assert.Null(
            service.Create(state));
    }

    [Fact]
    public void CreateFormatsNegativeZeroAsZero()
    {
        CalculationPreviewService service =
            CreateService();

        CalculatorState state =
            CreateState(
                0.0,
                BinaryOperator.Multiply,
                "-1");

        CalculationPreview preview =
            Assert.IsType<CalculationPreview>(
                service.Create(state));

        Assert.Equal(
            "0 × -1 =",
            preview.Expression);

        Assert.Equal(
            "0",
            preview.Result);
    }

    [Fact]
    public void ConstructorRejectsMissingCalculationEngine()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new CalculationPreviewService(
                    null!));

        Assert.Equal(
            "calculationEngine",
            exception.ParamName);
    }

    [Fact]
    public void CreateRejectsMissingState()
    {
        CalculationPreviewService service =
            CreateService();

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => service.Create(
                    null!));

        Assert.Equal(
            "state",
            exception.ParamName);
    }

    private static CalculationPreviewService CreateService()
    {
        return new CalculationPreviewService(
            new CalculationEngine());
    }

    private static CalculatorState CreateState(
        double leftOperand,
        BinaryOperator operation,
        string rightOperand)
    {
        return CalculatorState.Initial with
        {
            Editor =
                EditorState.FromText(
                    rightOperand),
            PendingOperation =
                new PendingBinaryOperation(
                    leftOperand,
                    operation),
            ReplaceDisplayOnNextInput = false,
        };
    }
}
