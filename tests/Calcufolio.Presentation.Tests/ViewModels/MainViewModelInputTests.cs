using Calcufolio.Application.Interaction.Editor.State;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Tests.ViewModels;

public sealed class MainViewModelInputTests
{
    [Fact]
    public void InitialStateShowsZeroAndNoExpression()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        Assert.Equal(
            "0",
            viewModel.DisplayValue);

        Assert.Equal(
            string.Empty,
            viewModel.Expression);

        Assert.Equal(
            1,
            viewModel.EditorCaretIndex);

        Assert.Empty(viewModel.HistoryEntries);
    }

    [Fact]
    public void ConstructorProjectsInitialSelection()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = new EditorState(
                    "12345",
                    1,
                    4),
            };

        MainViewModel viewModel =
            MainViewModelTestFactory.Create(initialState);

        Assert.Equal(
            1,
            viewModel.EditorCaretIndex);

        Assert.Equal(
            1,
            viewModel.EditorSelectionStart);

        Assert.Equal(
            4,
            viewModel.EditorSelectionEnd);
    }

    [Fact]
    public void AppendDigitReplacesInitialZero()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("7");

        Assert.Equal(
            "7",
            viewModel.DisplayValue);
    }

    [Fact]
    public void AppendDigitBuildsMultiDigitValue()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("1");
        viewModel.AppendDigitCommand.Execute("2");
        viewModel.AppendDigitCommand.Execute("3");

        Assert.Equal(
            "123",
            viewModel.DisplayValue);
    }

    [Fact]
    public void AppendDigitDoesNotCreateLeadingZeroes()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("0");
        viewModel.AppendDigitCommand.Execute("0");
        viewModel.AppendDigitCommand.Execute("7");

        Assert.Equal(
            "7",
            viewModel.DisplayValue);
    }

    [Fact]
    public void AppendDecimalSeparatorCreatesFractionalInput()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDecimalSeparatorCommand.Execute(null);
        viewModel.AppendDigitCommand.Execute("5");

        Assert.Equal(
            "0.5",
            viewModel.DisplayValue);
    }

    [Fact]
    public void AppendDecimalSeparatorIsIgnoredWhenAlreadyPresent()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("1");
        viewModel.AppendDecimalSeparatorCommand.Execute(null);
        viewModel.AppendDigitCommand.Execute("5");
        viewModel.AppendDecimalSeparatorCommand.Execute(null);

        Assert.Equal(
            "1.5",
            viewModel.DisplayValue);
    }

    [Fact]
    public void BackspaceRemovesPreviousCharacter()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("1");
        viewModel.AppendDigitCommand.Execute("2");
        viewModel.BackspaceCommand.Execute(null);

        Assert.Equal(
            "1",
            viewModel.DisplayValue);
    }

    [Fact]
    public void ClearResetsDisplayAndExpression()
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        viewModel.AppendDigitCommand.Execute("1");
        viewModel.SelectOperatorCommand.Execute("+");
        viewModel.AppendDigitCommand.Execute("2");
        viewModel.ClearCommand.Execute(null);

        Assert.Equal(
            "0",
            viewModel.DisplayValue);

        Assert.Equal(
            string.Empty,
            viewModel.Expression);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("x")]
    public void AppendDigitRejectsInvalidInput(
        string digit)
    {
        MainViewModel viewModel =
            MainViewModelTestFactory.Create();

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => viewModel.AppendDigitCommand.Execute(digit));

        Assert.Equal(
            "digit",
            exception.ParamName);
    }
}
