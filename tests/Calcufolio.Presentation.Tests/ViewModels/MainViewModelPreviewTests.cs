using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Tests.ViewModels;

public sealed class MainViewModelPreviewTests
{
    [Fact]
    public void InitialStateHasNoDisplayExpression()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        Assert.Equal(
            string.Empty,
            viewModel.DisplayExpression);
    }

    [Fact]
    public void PendingOperatorUsesTheExistingExpression()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "70");

        viewModel.SelectOperatorCommand.Execute("×");

        Assert.Equal(
            "70 ×",
            viewModel.DisplayExpression);

        Assert.Empty(
            viewModel.HistoryEntries);
    }

    [Fact]
    public void RightOperandPublishesLiveCalculationPreview()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "70");

        viewModel.SelectOperatorCommand.Execute("×");

        EnterNumber(
            viewModel,
            "279");

        Assert.Equal(
            "70 × 279 = 19530",
            viewModel.DisplayExpression);

        Assert.Equal(
            "279",
            viewModel.DisplayValue);

        Assert.Empty(
            viewModel.HistoryEntries);
    }

    [Fact]
    public void EditingRightOperandRefreshesPreview()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "70");

        viewModel.SelectOperatorCommand.Execute("×");

        EnterNumber(
            viewModel,
            "279");

        viewModel.BackspaceCommand.Execute(null);

        Assert.Equal(
            "70 × 27 = 1890",
            viewModel.DisplayExpression);

        Assert.Empty(
            viewModel.HistoryEntries);
    }

    [Fact]
    public void DivisionByZeroFallsBackToPendingExpression()
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

        Assert.Equal(
            "12 ÷",
            viewModel.DisplayExpression);

        Assert.Equal(
            "0",
            viewModel.DisplayValue);

        Assert.Empty(
            viewModel.HistoryEntries);
    }

    [Fact]
    public void EvaluateReturnsToCompletedExpressionAndStoresHistory()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "70");

        viewModel.SelectOperatorCommand.Execute("×");

        EnterNumber(
            viewModel,
            "279");

        viewModel.EvaluateCommand.Execute(null);

        Assert.Equal(
            "70 × 279 =",
            viewModel.DisplayExpression);

        Assert.Equal(
            "19530",
            viewModel.DisplayValue);

        Assert.Single(
            viewModel.HistoryEntries);
    }

    [Fact]
    public void ClearRemovesTheLivePreview()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        EnterNumber(
            viewModel,
            "70");

        viewModel.SelectOperatorCommand.Execute("×");

        EnterNumber(
            viewModel,
            "279");

        viewModel.ClearCommand.Execute(null);

        Assert.Equal(
            string.Empty,
            viewModel.DisplayExpression);

        Assert.Equal(
            "0",
            viewModel.DisplayValue);
    }

    private static void EnterNumber(
        MainViewModel viewModel,
        string value)
    {
        foreach (char character in value)
        {
            if (character == '.')
            {
                viewModel.AppendDecimalSeparatorCommand.Execute(
                    null);

                continue;
            }

            viewModel.AppendDigitCommand.Execute(
                character.ToString());
        }
    }
}
