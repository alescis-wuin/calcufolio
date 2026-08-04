using Calcufolio.Presentation.Input;

namespace Calcufolio.Presentation.Tests.Input;

public sealed class TextSelectionSnapshotTests
{
    [Fact]
    public void CreateUsesEndAsAnchorWhenCaretIsAtStart()
    {
        TextSelectionSnapshot selection =
            TextSelectionSnapshot.Create(
                5,
                1,
                4,
                1);

        Assert.Equal(
            4,
            selection.AnchorIndex);

        Assert.Equal(
            1,
            selection.CaretIndex);
    }

    [Fact]
    public void CreateUsesStartAsAnchorWhenCaretIsAtEnd()
    {
        TextSelectionSnapshot selection =
            TextSelectionSnapshot.Create(
                5,
                1,
                4,
                4);

        Assert.Equal(
            1,
            selection.AnchorIndex);

        Assert.Equal(
            4,
            selection.CaretIndex);
    }

    [Fact]
    public void CreateCollapsesAnchorToCaretWithoutSelection()
    {
        TextSelectionSnapshot selection =
            TextSelectionSnapshot.Create(
                5,
                3,
                3,
                3);

        Assert.Equal(
            3,
            selection.AnchorIndex);

        Assert.Equal(
            3,
            selection.CaretIndex);
    }

    [Fact]
    public void CreateClampsIndexesToTextLength()
    {
        TextSelectionSnapshot selection =
            TextSelectionSnapshot.Create(
                5,
                -10,
                20,
                20);

        Assert.Equal(
            0,
            selection.AnchorIndex);

        Assert.Equal(
            5,
            selection.CaretIndex);
    }

    [Fact]
    public void CreateRejectsNegativeTextLength()
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => TextSelectionSnapshot.Create(
                    -1,
                    0,
                    0,
                    0));

        Assert.Equal(
            "textLength",
            exception.ParamName);
    }
}
