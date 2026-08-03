using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.Editor.Actions;
using Calcufolio.Application.Interaction.Editor.Reducer;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;

namespace Calcufolio.Application.Tests.Interaction.Controller;

public sealed class CalculatorControllerIncompleteInputTests
{
    [Fact]
    public void EvaluateWithEmptyEditedOperandDoesNothing()
    {
        TestContext context = CreateContext();

        context.Controller.Dispatch(
            new AppendDigitAction("3"));

        context.Controller.Dispatch(
            new SelectOperatorAction("+"));

        context.Controller.Dispatch(
            new EditInputAction(
                new BackspaceEditorAction()));

        context.Controller.Dispatch(
            new EvaluateAction());

        CalculatorState state =
            context.StateStore.Current;

        Assert.Equal(
            string.Empty,
            state.DisplayValue);

        Assert.Equal(
            "3 +",
            state.Expression);

        Assert.NotNull(
            state.PendingOperation);

        Assert.Empty(
            state.HistoryEntries);

        Assert.False(
            state.HasError);
    }

    [Fact]
    public void SelectingOperatorWithEmptyEditedOperandDoesNothing()
    {
        TestContext context = CreateContext();

        context.Controller.Dispatch(
            new AppendDigitAction("3"));

        context.Controller.Dispatch(
            new SelectOperatorAction("+"));

        context.Controller.Dispatch(
            new EditInputAction(
                new BackspaceEditorAction()));

        context.Controller.Dispatch(
            new SelectOperatorAction("×"));

        CalculatorState state =
            context.StateStore.Current;

        Assert.Equal(
            string.Empty,
            state.DisplayValue);

        Assert.Equal(
            "3 +",
            state.Expression);

        Assert.Equal(
            BinaryOperator.Add,
            state.PendingOperation?.Operation);

        Assert.Empty(
            state.HistoryEntries);
    }

    [Fact]
    public void DecimalSeparatorAfterEmptyOperandCreatesZeroFraction()
    {
        TestContext context = CreateContext();

        context.Controller.Dispatch(
            new AppendDigitAction("3"));

        context.Controller.Dispatch(
            new SelectOperatorAction("+"));

        context.Controller.Dispatch(
            new EditInputAction(
                new BackspaceEditorAction()));

        context.Controller.Dispatch(
            new AppendDecimalSeparatorAction());

        Assert.Equal(
            "0.",
            context.StateStore.Current.DisplayValue);
    }

    private static TestContext CreateContext()
    {
        CalculatorStateStore stateStore = new();

        CalculatorController controller = new(
            new CalculationEngine(),
            new EditorStateReducer(),
            stateStore);

        return new TestContext(
            controller,
            stateStore);
    }

    private sealed record TestContext(
        ICalculatorController Controller,
        ICalculatorStateStore StateStore);
}
