using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Tests.ViewModels;

public sealed class MainViewModelOperatorTests
{
    [Fact]
    public void SelectOperatorCapturesLeftOperandAndUpdatesExpression()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("1");
        viewModel.AppendDigitCommand.Execute("2");
        viewModel.SelectOperatorCommand.Execute("+");

        Assert.Equal(
            "12 +",
            viewModel.Expression);

        Assert.Equal(
            "12",
            viewModel.DisplayValue);
    }

    [Fact]
    public void AppendDigitAfterOperatorStartsRightOperand()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("1");
        viewModel.AppendDigitCommand.Execute("2");
        viewModel.SelectOperatorCommand.Execute("+");
        viewModel.AppendDigitCommand.Execute("7");

        Assert.Equal(
            "7",
            viewModel.DisplayValue);

        Assert.Equal(
            "12 +",
            viewModel.Expression);
    }

    [Fact]
    public void AppendDecimalSeparatorAfterOperatorStartsFractionalOperand()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("8");
        viewModel.SelectOperatorCommand.Execute("÷");
        viewModel.AppendDecimalSeparatorCommand.Execute(null);
        viewModel.AppendDigitCommand.Execute("5");

        Assert.Equal(
            "0.5",
            viewModel.DisplayValue);

        Assert.Equal(
            "8 ÷",
            viewModel.Expression);
    }

    [Fact]
    public void SelectingOperatorAgainReplacesPendingOperator()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("4");
        viewModel.AppendDigitCommand.Execute("2");
        viewModel.SelectOperatorCommand.Execute("+");
        viewModel.SelectOperatorCommand.Execute("×");

        Assert.Equal(
            "42 ×",
            viewModel.Expression);

        Assert.Equal(
            "42",
            viewModel.DisplayValue);
    }

    [Fact]
    public void ClearResetsPendingOperatorState()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("9");
        viewModel.SelectOperatorCommand.Execute("−");
        viewModel.ClearCommand.Execute(null);
        viewModel.AppendDigitCommand.Execute("3");

        Assert.Equal(
            "3",
            viewModel.DisplayValue);

        Assert.Equal(
            string.Empty,
            viewModel.Expression);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("/")]
    public void SelectOperatorRejectsUnsupportedSymbol(string operatorSymbol)
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => viewModel.SelectOperatorCommand.Execute(operatorSymbol));

        Assert.Equal(
            "operatorSymbol",
            exception.ParamName);
    }
}
