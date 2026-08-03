using Calcufolio.Application.Interaction.Editor.Actions;
using Calcufolio.Application.Interaction.Editor.Reducer;
using Calcufolio.Application.Interaction.Editor.State;

namespace Calcufolio.Application.Tests.Interaction.Editor.Reducer;

public sealed class EditorStateReducerTests
{
    private readonly EditorStateReducer _reducer = new();

    [Fact]
    public void InsertTextAddsTextAtCaret()
    {
        EditorState state = new(
            "12",
            1,
            1);

        EditorState result = _reducer.Reduce(
            state,
            new InsertTextEditorAction("3"));

        Assert.Equal(
            "132",
            result.Text);

        Assert.Equal(
            2,
            result.CaretIndex);

        Assert.False(result.HasSelection);
    }

    [Fact]
    public void InsertTextReplacesSelection()
    {
        EditorState state = new(
            "12345",
            4,
            1);

        EditorState result = _reducer.Reduce(
            state,
            new InsertTextEditorAction("9"));

        Assert.Equal(
            "195",
            result.Text);

        Assert.Equal(
            2,
            result.CaretIndex);

        Assert.False(result.HasSelection);
    }

    [Fact]
    public void BackspaceDeletesSelection()
    {
        EditorState state = new(
            "12345",
            1,
            4);

        EditorState result = _reducer.Reduce(
            state,
            new BackspaceEditorAction());

        Assert.Equal(
            "15",
            result.Text);

        Assert.Equal(
            1,
            result.CaretIndex);
    }

    [Fact]
    public void BackspaceDeletesCharacterBeforeCaret()
    {
        EditorState state = new(
            "123",
            2,
            2);

        EditorState result = _reducer.Reduce(
            state,
            new BackspaceEditorAction());

        Assert.Equal(
            "13",
            result.Text);

        Assert.Equal(
            1,
            result.CaretIndex);
    }

    [Fact]
    public void BackspaceAtStartReturnsSameState()
    {
        EditorState state = new(
            "123",
            0,
            0);

        EditorState result = _reducer.Reduce(
            state,
            new BackspaceEditorAction());

        Assert.Same(
            state,
            result);
    }

    [Fact]
    public void DeleteForwardDeletesSelection()
    {
        EditorState state = new(
            "12345",
            4,
            1);

        EditorState result = _reducer.Reduce(
            state,
            new DeleteForwardEditorAction());

        Assert.Equal(
            "15",
            result.Text);

        Assert.Equal(
            1,
            result.CaretIndex);
    }

    [Fact]
    public void DeleteForwardDeletesCharacterAtCaret()
    {
        EditorState state = new(
            "123",
            1,
            1);

        EditorState result = _reducer.Reduce(
            state,
            new DeleteForwardEditorAction());

        Assert.Equal(
            "13",
            result.Text);

        Assert.Equal(
            1,
            result.CaretIndex);
    }

    [Fact]
    public void DeleteForwardAtEndReturnsSameState()
    {
        EditorState state = new(
            "123",
            3,
            3);

        EditorState result = _reducer.Reduce(
            state,
            new DeleteForwardEditorAction());

        Assert.Same(
            state,
            result);
    }

    [Fact]
    public void MoveCaretClampsToTextBoundaries()
    {
        EditorState state = new(
            "123",
            1,
            1);

        EditorState leftResult = _reducer.Reduce(
            state,
            new MoveCaretEditorAction(
                -10,
                false));

        EditorState rightResult = _reducer.Reduce(
            state,
            new MoveCaretEditorAction(
                10,
                false));

        Assert.Equal(
            0,
            leftResult.CaretIndex);

        Assert.Equal(
            3,
            rightResult.CaretIndex);
    }

    [Fact]
    public void MoveCaretWithoutExtensionCollapsesSelection()
    {
        EditorState state = new(
            "12345",
            4,
            1);

        EditorState leftResult = _reducer.Reduce(
            state,
            new MoveCaretEditorAction(
                -1,
                false));

        EditorState rightResult = _reducer.Reduce(
            state,
            new MoveCaretEditorAction(
                1,
                false));

        Assert.Equal(
            1,
            leftResult.CaretIndex);

        Assert.Equal(
            4,
            rightResult.CaretIndex);

        Assert.False(leftResult.HasSelection);
        Assert.False(rightResult.HasSelection);
    }

    [Fact]
    public void MoveCaretWithExtensionKeepsSelectionAnchor()
    {
        EditorState state = new(
            "12345",
            2,
            2);

        EditorState result = _reducer.Reduce(
            state,
            new MoveCaretEditorAction(
                2,
                true));

        Assert.Equal(
            4,
            result.CaretIndex);

        Assert.Equal(
            2,
            result.SelectionAnchorIndex);

        Assert.Equal(
            "34",
            result.SelectedText);
    }

    [Fact]
    public void MoveCaretToBoundariesSupportsSelectionExtension()
    {
        EditorState state = new(
            "12345",
            2,
            2);

        EditorState startResult = _reducer.Reduce(
            state,
            new MoveCaretToStartEditorAction(true));

        EditorState endResult = _reducer.Reduce(
            state,
            new MoveCaretToEndEditorAction(true));

        Assert.Equal(
            "12",
            startResult.SelectedText);

        Assert.Equal(
            "345",
            endResult.SelectedText);
    }

    [Fact]
    public void SetSelectionClampsPointerIndexes()
    {
        EditorState state = new(
            "12345",
            0,
            0);

        EditorState result = _reducer.Reduce(
            state,
            new SetSelectionEditorAction(
                -10,
                20));

        Assert.Equal(
            0,
            result.SelectionStart);

        Assert.Equal(
            5,
            result.SelectionEnd);
    }

    [Fact]
    public void SelectAllSelectsCompleteText()
    {
        EditorState state = new(
            "12345",
            2,
            2);

        EditorState result = _reducer.Reduce(
            state,
            new SelectAllEditorAction());

        Assert.Equal(
            "12345",
            result.SelectedText);

        Assert.Equal(
            5,
            result.CaretIndex);

        Assert.Equal(
            0,
            result.SelectionAnchorIndex);
    }

    [Fact]
    public void ReduceRejectsMissingState()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _reducer.Reduce(
                    null!,
                    new SelectAllEditorAction()));

        Assert.Equal(
            "state",
            exception.ParamName);
    }

    [Fact]
    public void ReduceRejectsMissingAction()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => _reducer.Reduce(
                    EditorState.Empty,
                    null!));

        Assert.Equal(
            "action",
            exception.ParamName);
    }
}
