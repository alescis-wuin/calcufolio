using System.Globalization;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;
using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Tests.ViewModels;

public sealed class MainViewModelEvaluationTests
{
    [Fact]
    public void EvaluateAddsOperandsAndRecordsHistory()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "12");

        viewModel.SelectOperatorCommand.Execute("+");

        EnterNumber(
            viewModel,
            "7");

        viewModel.EvaluateCommand.Execute(null);

        Assert.Equal(
            "19",
            viewModel.DisplayValue);

        Assert.Equal(
            "12 + 7 =",
            viewModel.Expression);

        CalculationHistoryEntryViewModel historyEntry =
            Assert.Single(viewModel.HistoryEntries);

        Assert.Equal(
            "12 + 7 =",
            historyEntry.Expression);

        Assert.Equal(
            "19",
            historyEntry.Result);
    }

    [Theory]
    [InlineData("12", "+", "7", "19")]
    [InlineData("12", "−", "7", "5")]
    [InlineData("12", "×", "7", "84")]
    [InlineData("12", "÷", "4", "3")]
    public void EvaluateSupportsBinaryOperators(
        string leftOperand,
        string operatorSymbol,
        string rightOperand,
        string expectedResult)
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            leftOperand);

        viewModel.SelectOperatorCommand.Execute(
            operatorSymbol);

        EnterNumber(
            viewModel,
            rightOperand);

        viewModel.EvaluateCommand.Execute(null);

        Assert.Equal(
            expectedResult,
            viewModel.DisplayValue);
    }

    [Fact]
    public void EvaluateWithoutPendingOperationDoesNothing()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "7");

        viewModel.EvaluateCommand.Execute(null);

        Assert.Equal(
            "7",
            viewModel.DisplayValue);

        Assert.Equal(
            string.Empty,
            viewModel.Expression);

        Assert.Empty(viewModel.HistoryEntries);
    }

    [Fact]
    public void EvaluateWithoutRightOperandDoesNothing()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "12");

        viewModel.SelectOperatorCommand.Execute("+");
        viewModel.EvaluateCommand.Execute(null);

        Assert.Equal(
            "12",
            viewModel.DisplayValue);

        Assert.Equal(
            "12 +",
            viewModel.Expression);

        Assert.Empty(viewModel.HistoryEntries);
    }

    [Fact]
    public void SelectingOperatorAfterRightOperandChainsPreviousResult()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "12");

        viewModel.SelectOperatorCommand.Execute("+");

        EnterNumber(
            viewModel,
            "7");

        viewModel.SelectOperatorCommand.Execute("×");

        Assert.Equal(
            "19",
            viewModel.DisplayValue);

        Assert.Equal(
            "19 ×",
            viewModel.Expression);

        EnterNumber(
            viewModel,
            "2");

        viewModel.EvaluateCommand.Execute(null);

        Assert.Equal(
            "38",
            viewModel.DisplayValue);

        Assert.Equal(
            2,
            viewModel.HistoryEntries.Count);
    }

    [Fact]
    public void DivisionByZeroShowsRecoverableError()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "12");

        viewModel.SelectOperatorCommand.Execute("÷");

        EnterNumber(
            viewModel,
            "0");

        viewModel.EvaluateCommand.Execute(null);

        Assert.Equal(
            "Error",
            viewModel.DisplayValue);

        Assert.Equal(
            "Division by zero is not allowed.",
            viewModel.Expression);

        Assert.Empty(viewModel.HistoryEntries);
    }

    [Fact]
    public void DigitAfterErrorStartsFreshCalculation()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "12");

        viewModel.SelectOperatorCommand.Execute("÷");

        EnterNumber(
            viewModel,
            "0");

        viewModel.EvaluateCommand.Execute(null);
        viewModel.AppendDigitCommand.Execute("7");

        Assert.Equal(
            "7",
            viewModel.DisplayValue);

        Assert.Equal(
            string.Empty,
            viewModel.Expression);
    }

    [Fact]
    public void DigitAfterResultStartsFreshCalculation()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "12");

        viewModel.SelectOperatorCommand.Execute("+");

        EnterNumber(
            viewModel,
            "7");

        viewModel.EvaluateCommand.Execute(null);
        viewModel.AppendDigitCommand.Execute("3");

        Assert.Equal(
            "3",
            viewModel.DisplayValue);

        Assert.Equal(
            string.Empty,
            viewModel.Expression);

        Assert.Single(viewModel.HistoryEntries);
    }

    [Fact]
    public void OverflowShowsRecoverableError()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                DisplayValue = double.MaxValue.ToString(
                    "R",
                    CultureInfo.InvariantCulture),
            };

        MainViewModel viewModel =
            MainViewModelTestFactory.Create(initialState);

        viewModel.SelectOperatorCommand.Execute("×");
        viewModel.AppendDigitCommand.Execute("2");
        viewModel.EvaluateCommand.Execute(null);

        Assert.Equal(
            "Error",
            viewModel.DisplayValue);

        Assert.Equal(
            "The calculation result is outside the supported numeric range.",
            viewModel.Expression);
    }

    [Fact]
    public void HistoryKeepsMostRecentTwentyCalculations()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        for (int index = 0; index < 21; index++)
        {
            viewModel.AppendDigitCommand.Execute("1");
            viewModel.SelectOperatorCommand.Execute("+");
            viewModel.AppendDigitCommand.Execute("1");
            viewModel.EvaluateCommand.Execute(null);
        }

        Assert.Equal(
            20,
            viewModel.HistoryEntries.Count);
    }

    [Fact]
    public void ConstructorRejectsMissingController()
    {
        CalculatorStateStore stateStore = new();

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new MainViewModel(
                    null!,
                    stateStore));

        Assert.Equal(
            "controller",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorRejectsMissingStateStore()
    {
        CalculatorStateStore stateStore = new();

        CalculatorController controller = new(
            new CalculationEngine(),
            stateStore);

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new MainViewModel(
                    controller,
                    null!));

        Assert.Equal(
            "stateStore",
            exception.ParamName);
    }

    private static void EnterNumber(
        MainViewModel viewModel,
        string value)
    {
        foreach (char character in value)
        {
            if (character == '.')
            {
                viewModel.AppendDecimalSeparatorCommand.Execute(null);
                continue;
            }

            viewModel.AppendDigitCommand.Execute(
                character.ToString());
        }
    }
}
