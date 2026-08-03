using Calcufolio.Application.Interaction.Editor.State;

namespace Calcufolio.Application.Tests.Interaction.Editor.State;

public sealed class EditorStateTests
{
    [Fact]
    public void EmptyStateHasNoTextOrSelection()
    {
        EditorState state =
            EditorState.Empty;

        Assert.Equal(
            string.Empty,
            state.Text);

        Assert.Equal(
            0,
            state.CaretIndex);

        Assert.Equal(
            0,
            state.SelectionLength);

        Assert.False(state.HasSelection);
    }

    [Fact]
    public void ForwardSelectionExposesNormalizedRange()
    {
        EditorState state = new(
            "12345",
            4,
            1);

        Assert.Equal(
            1,
            state.SelectionStart);

        Assert.Equal(
            4,
            state.SelectionEnd);

        Assert.Equal(
            "234",
            state.SelectedText);
    }

    [Fact]
    public void ReversedSelectionExposesNormalizedRange()
    {
        EditorState state = new(
            "12345",
            1,
            4);

        Assert.Equal(
            1,
            state.SelectionStart);

        Assert.Equal(
            4,
            state.SelectionEnd);

        Assert.Equal(
            "234",
            state.SelectedText);
    }

    [Fact]
    public void ConstructorRejectsMissingText()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new EditorState(
                    null!,
                    0,
                    0));

        Assert.Equal(
            "text",
            exception.ParamName);
    }

    [Theory]
    [InlineData(-1, 0, "caretIndex")]
    [InlineData(4, 0, "caretIndex")]
    [InlineData(0, -1, "selectionAnchorIndex")]
    [InlineData(0, 4, "selectionAnchorIndex")]
    public void ConstructorRejectsOutOfRangeIndexes(
        int caretIndex,
        int selectionAnchorIndex,
        string expectedParameterName)
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new EditorState(
                    "123",
                    caretIndex,
                    selectionAnchorIndex));

        Assert.Equal(
            expectedParameterName,
            exception.ParamName);
    }
}
