using Calcufolio.Application.Expressions;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Clipboard;
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

namespace Calcufolio.Application.Tests.Interaction.Clipboard;

public sealed class CalculatorClipboardControllerTests
{
    [Fact]
    public async Task CopyWritesSelectedText()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = new EditorState(
                    "12345",
                    4,
                    1),
            };

        ClipboardTestContext context =
            CreateContext(initialState);

        await context.ClipboardController.CopyAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "234",
            context.ClipboardPort.WrittenText);
    }

    [Fact]
    public async Task CopyWritesCompleteDisplayWithoutSelection()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = EditorState.FromText("12345"),
            };

        ClipboardTestContext context =
            CreateContext(initialState);

        await context.ClipboardController.CopyAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "12345",
            context.ClipboardPort.WrittenText);
    }

    [Fact]
    public async Task CutWritesSelectionAndDeletesIt()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = new EditorState(
                    "12345",
                    4,
                    1),
            };

        ClipboardTestContext context =
            CreateContext(initialState);

        await context.ClipboardController.CutAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "234",
            context.ClipboardPort.WrittenText);

        Assert.Equal(
            "15",
            context.StateStore.Current.DisplayValue);

        Assert.Equal(
            1,
            context.StateStore.Current.Editor.CaretIndex);

        Assert.False(
            context.StateStore.Current.Editor.HasSelection);
    }

    [Fact]
    public async Task CutWithoutSelectionDoesNothing()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = EditorState.FromText("12345"),
            };

        ClipboardTestContext context =
            CreateContext(initialState);

        await context.ClipboardController.CutAsync(
            TestContext.Current.CancellationToken);

        Assert.Null(
            context.ClipboardPort.WrittenText);

        Assert.Equal(
            "12345",
            context.StateStore.Current.DisplayValue);
    }

    [Fact]
    public async Task CutDoesNotDeleteChangedEditorState()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = new EditorState(
                    "12345",
                    4,
                    1),
            };

        ClipboardTestContext context =
            CreateContext(initialState);

        context.ClipboardPort.AfterWrite = () =>
        {
            context.CalculatorController.Dispatch(
                new EditInputAction(
                    new InsertTextEditorAction("9")));
        };

        await context.ClipboardController.CutAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "234",
            context.ClipboardPort.WrittenText);

        Assert.Equal(
            "195",
            context.StateStore.Current.DisplayValue);
    }

    [Fact]
    public async Task PasteReplacesInitialZero()
    {
        ClipboardTestContext context =
            CreateContext(
                CalculatorState.Initial,
                "42");

        await context.ClipboardController.PasteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "42",
            context.StateStore.Current.DisplayValue);
    }

    [Fact]
    public async Task PasteInsertsTextAtCaret()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = new EditorState(
                    "123",
                    1,
                    1),
            };

        ClipboardTestContext context =
            CreateContext(
                initialState,
                "45");

        await context.ClipboardController.PasteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "14523",
            context.StateStore.Current.DisplayValue);

        Assert.Equal(
            3,
            context.StateStore.Current.Editor.CaretIndex);
    }

    [Fact]
    public async Task PasteReplacesCurrentSelection()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = new EditorState(
                    "12345",
                    4,
                    1),
            };

        ClipboardTestContext context =
            CreateContext(
                initialState,
                "9");

        await context.ClipboardController.PasteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "195",
            context.StateStore.Current.DisplayValue);

        Assert.False(
            context.StateStore.Current.Editor.HasSelection);
    }

    [Fact]
    public async Task PasteNormalizesDecimalComma()
    {
        ClipboardTestContext context =
            CreateContext(
                CalculatorState.Initial,
                "12,5");

        context.CalculatorController.Dispatch(
            new EditInputAction(
                new SelectAllEditorAction()));

        await context.ClipboardController.PasteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "12.5",
            context.StateStore.Current.DisplayValue);
    }

    [Fact]
    public async Task PasteIgnoresInvalidText()
    {
        ClipboardTestContext context =
            CreateContext(
                CalculatorState.Initial,
                "2 + 3");

        await context.ClipboardController.PasteAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(
            "0",
            context.StateStore.Current.DisplayValue);
    }

    [Fact]
    public async Task CopyDoesNothingForEmptyDisplay()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                Editor = EditorState.Empty,
            };

        ClipboardTestContext context =
            CreateContext(initialState);

        await context.ClipboardController.CopyAsync(
            TestContext.Current.CancellationToken);

        Assert.Null(
            context.ClipboardPort.WrittenText);
    }

    private static ClipboardTestContext CreateContext(
        CalculatorState initialState,
        string? clipboardText = null)
    {
        CalculatorStateStore stateStore =
            new(initialState);

        CalculatorController calculatorController = new(
            CreateExpressionEvaluationService(),
            new EditorStateReducer(),
            stateStore);

        FakeClipboardPort clipboardPort =
            new(clipboardText);

        CalculatorClipboardController clipboardController = new(
            calculatorController,
            stateStore,
            clipboardPort,
            new NumericClipboardTextSanitizer());

        return new ClipboardTestContext(
            calculatorController,
            clipboardController,
            stateStore,
            clipboardPort);
    }

    private static ExpressionEvaluationService CreateExpressionEvaluationService()
    {
        ExpressionParser parser =
            new(
                new ExpressionTokenizer());

        ExpressionEvaluator evaluator =
            new(
                new CalculationEngine());

        ExpressionEngine engine =
            new(
                parser,
                evaluator);

        return new ExpressionEvaluationService(
            engine);
    }

    private sealed record ClipboardTestContext(
        ICalculatorController CalculatorController,
        ICalculatorClipboardController ClipboardController,
        ICalculatorStateStore StateStore,
        FakeClipboardPort ClipboardPort);

    private sealed class FakeClipboardPort : IClipboardPort
    {
        public FakeClipboardPort(
            string? text)
        {
            Text = text;
        }

        public Action? AfterWrite { get; set; }

        public string? Text { get; }

        public string? WrittenText { get; private set; }

        public ValueTask<string?> ReadTextAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return ValueTask.FromResult(Text);
        }

        public ValueTask WriteTextAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            WrittenText = text;
            AfterWrite?.Invoke();

            return ValueTask.CompletedTask;
        }
    }
}
