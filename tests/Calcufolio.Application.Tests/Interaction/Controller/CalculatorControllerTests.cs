using System.Globalization;
using Calcufolio.Application.Calculations;
using Calcufolio.Application.Expressions;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.Editor.Actions;
using Calcufolio.Application.Interaction.Editor.Reducer;
using Calcufolio.Application.Interaction.Editor.State;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;
using Calcufolio.Domain.Expressions;
using Calcufolio.Domain.Expressions.Evaluation;
using Calcufolio.Domain.Expressions.Lexing;
using Calcufolio.Domain.Expressions.Parsing;

namespace Calcufolio.Application.Tests.Interaction.Controller;

public sealed class CalculatorControllerTests
{
    [Fact]
    public void AppendDigitUpdatesDisplay()
    {
        TestContext context = CreateContext();

        context.Controller.Dispatch(
            new AppendDigitAction("7"));

        Assert.Equal(
            "7",
            context.StateStore.Current.DisplayValue);
    }

    [Fact]
    public void EditInputAppliesSelectionAndInsertion()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = EditorState.FromText("42"),
            };

        TestContext context =
            CreateContext(initialState);

        context.Controller.Dispatch(
            new EditInputAction(
                new SetSelectionEditorAction(
                    0,
                    2)));

        context.Controller.Dispatch(
            new EditInputAction(
                new InsertTextEditorAction("7")));

        Assert.Equal(
            "7",
            context.StateStore.Current.DisplayValue);

        Assert.Equal(
            1,
            context.StateStore.Current.Editor.CaretIndex);

        Assert.False(
            context.StateStore.Current.Editor.HasSelection);
    }

    [Fact]
    public void EditInputMovementKeepsDisplayText()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = EditorState.FromText("123"),
                ReplaceDisplayOnNextInput = true,
            };

        TestContext context =
            CreateContext(initialState);

        context.Controller.Dispatch(
            new EditInputAction(
                new MoveCaretEditorAction(
                    -1,
                    false)));

        Assert.Equal(
            "123",
            context.StateStore.Current.DisplayValue);

        Assert.Equal(
            2,
            context.StateStore.Current.Editor.CaretIndex);

        Assert.False(
            context.StateStore.Current.ReplaceDisplayOnNextInput);
    }

    [Fact]
    public void SelectOperatorStoresPendingOperation()
    {
        TestContext context = CreateContext();

        context.Controller.Dispatch(
            new AppendDigitAction("4"));

        context.Controller.Dispatch(
            new SelectOperatorAction("+"));

        PendingBinaryOperation pendingOperation =
            Assert.IsType<PendingBinaryOperation>(
                context.StateStore.Current.PendingOperation);

        Assert.Equal(
            4.0,
            pendingOperation.LeftOperand);

        Assert.Equal(
            BinaryOperator.Add,
            pendingOperation.Operation);

        Assert.Equal(
            "4 +",
            context.StateStore.Current.Expression);
    }

    [Fact]
    public void EvaluateUpdatesDisplayAndHistory()
    {
        TestContext context = CreateContext();

        EnterNumber(
            context.Controller,
            "12");

        context.Controller.Dispatch(
            new SelectOperatorAction("+"));

        EnterNumber(
            context.Controller,
            "7");

        context.Controller.Dispatch(
            new EvaluateAction());

        CalculatorState state =
            context.StateStore.Current;

        Assert.Equal(
            "19",
            state.DisplayValue);

        Assert.Equal(
            "12 + 7 =",
            state.Expression);

        CalculationHistoryEntry historyEntry =
            Assert.Single(state.HistoryEntries);

        Assert.Equal(
            "12 + 7 =",
            historyEntry.Expression);

        Assert.Equal(
            "19",
            historyEntry.Result);
    }

    [Fact]
    public void SelectOperatorChainsPendingCalculation()
    {
        TestContext context = CreateContext();

        EnterNumber(
            context.Controller,
            "12");

        context.Controller.Dispatch(
            new SelectOperatorAction("+"));

        EnterNumber(
            context.Controller,
            "7");

        context.Controller.Dispatch(
            new SelectOperatorAction("×"));

        Assert.Equal(
            "19",
            context.StateStore.Current.DisplayValue);

        Assert.Equal(
            "19 ×",
            context.StateStore.Current.Expression);

        Assert.Single(
            context.StateStore.Current.HistoryEntries);
    }

    [Fact]
    public void DivisionByZeroPublishesRecoverableError()
    {
        TestContext context = CreateContext();

        EnterNumber(
            context.Controller,
            "12");

        context.Controller.Dispatch(
            new SelectOperatorAction("÷"));

        EnterNumber(
            context.Controller,
            "0");

        context.Controller.Dispatch(
            new EvaluateAction());

        CalculatorState state =
            context.StateStore.Current;

        Assert.True(state.HasError);

        Assert.Equal(
            "Error",
            state.DisplayValue);

        Assert.Contains(
            "Division by zero is not allowed",
            state.Expression,
            StringComparison.Ordinal);

        Assert.Null(state.PendingOperation);
    }

    [Fact]
    public void DigitAfterErrorStartsFreshCalculation()
    {
        TestContext context = CreateContext();

        EnterNumber(
            context.Controller,
            "12");

        context.Controller.Dispatch(
            new SelectOperatorAction("÷"));

        EnterNumber(
            context.Controller,
            "0");

        context.Controller.Dispatch(
            new EvaluateAction());

        context.Controller.Dispatch(
            new AppendDigitAction("7"));

        Assert.Equal(
            "7",
            context.StateStore.Current.DisplayValue);

        Assert.Equal(
            string.Empty,
            context.StateStore.Current.Expression);

        Assert.False(
            context.StateStore.Current.HasError);
    }

    [Fact]
    public void ClearPreservesHistoryAndResetsInteractionState()
    {
        TestContext context = CreateContext();

        EnterNumber(
            context.Controller,
            "1");

        context.Controller.Dispatch(
            new SelectOperatorAction("+"));

        EnterNumber(
            context.Controller,
            "1");

        context.Controller.Dispatch(
            new EvaluateAction());

        context.Controller.Dispatch(
            new ClearAction());

        CalculatorState state =
            context.StateStore.Current;

        Assert.Equal(
            "0",
            state.DisplayValue);

        Assert.Equal(
            string.Empty,
            state.Expression);

        Assert.Null(state.PendingOperation);

        Assert.Single(state.HistoryEntries);
    }

    [Fact]
    public void HistoryKeepsMostRecentTwentyCalculations()
    {
        TestContext context = CreateContext();

        for (int index = 0; index < 21; index++)
        {
            context.Controller.Dispatch(
                new AppendDigitAction("1"));

            context.Controller.Dispatch(
                new SelectOperatorAction("+"));

            context.Controller.Dispatch(
                new AppendDigitAction("1"));

            context.Controller.Dispatch(
                new EvaluateAction());
        }

        Assert.Equal(
            20,
            context.StateStore.Current.HistoryEntries.Count);
    }

    [Fact]
    public void OverflowPublishesRecoverableError()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = EditorState.FromText(
                    double.MaxValue.ToString(
                        "R",
                        CultureInfo.InvariantCulture)),
            };

        TestContext context =
            CreateContext(initialState);

        context.Controller.Dispatch(
            new SelectOperatorAction("×"));

        context.Controller.Dispatch(
            new AppendDigitAction("2"));

        context.Controller.Dispatch(
            new EvaluateAction());

        Assert.Equal(
            "Error",
            context.StateStore.Current.DisplayValue);

        Assert.Contains(
            "The calculation result is outside the supported numeric range",
            context.StateStore.Current.Expression,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ConstructorRejectsMissingExpressionEvaluationService()
    {
        EditorStateReducer editorStateReducer = new();
        CalculatorStateStore stateStore = new();

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new CalculatorController(
                    null!,
                    editorStateReducer,
                    stateStore));

        Assert.Equal(
            "expressionEvaluationService",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorRejectsMissingEditorStateReducer()
    {
        CalculatorStateStore stateStore = new();

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new CalculatorController(
                    CreateExpressionEvaluationService(),
                    null!,
                    stateStore));

        Assert.Equal(
            "editorStateReducer",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorRejectsMissingStateStore()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new CalculatorController(
                    CreateExpressionEvaluationService(),
                    new EditorStateReducer(),
                    null!));

        Assert.Equal(
            "stateStore",
            exception.ParamName);
    }

    private static TestContext CreateContext(
        CalculatorState? initialState = null)
    {
        CalculatorStateStore stateStore = initialState is null
            ? new CalculatorStateStore()
            : new CalculatorStateStore(initialState);

        CalculatorController controller = new(
            CreateExpressionEvaluationService(),
            new EditorStateReducer(),
            stateStore);

        return new TestContext(
            controller,
            stateStore);
    }

    private static ExpressionEvaluationService CreateExpressionEvaluationService()
    {
        IExpressionTokenizer tokenizer =
            new ExpressionTokenizer();

        IExpressionParser parser =
            new ExpressionParser(
                tokenizer);

        IExpressionEvaluator evaluator =
            new ExpressionEvaluator(
                new CalculationEngine());

        IExpressionEngine engine =
            new ExpressionEngine(
                parser,
                evaluator);

        return new ExpressionEvaluationService(
            engine);
    }

    private static void EnterNumber(
        ICalculatorController controller,
        string value)
    {
        foreach (char character in value)
        {
            if (character == '.')
            {
                controller.Dispatch(
                    new AppendDecimalSeparatorAction());

                continue;
            }

            controller.Dispatch(
                new AppendDigitAction(
                    character.ToString()));
        }
    }

    private sealed record TestContext(
        ICalculatorController Controller,
        ICalculatorStateStore StateStore);
}
