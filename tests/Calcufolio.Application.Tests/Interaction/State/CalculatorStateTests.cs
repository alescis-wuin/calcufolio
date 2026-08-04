using Calcufolio.Application.Interaction.Editor.State;
using Calcufolio.Application.Interaction.State;

namespace Calcufolio.Application.Tests.Interaction.State;

public sealed class CalculatorStateTests
{
    [Fact]
    public void InitialStateUsesEditorTextAsDisplayValue()
    {
        CalculatorState state =
            CalculatorState.Initial;

        Assert.Equal(
            "0",
            state.Editor.Text);

        Assert.Equal(
            state.Editor.Text,
            state.DisplayValue);
    }

    [Fact]
    public void DisplayValueReflectsReplacedEditorState()
    {
        CalculatorState state =
            CalculatorState.Initial with
            {
                Editor = EditorState.FromText("42"),
            };

        Assert.Equal(
            "42",
            state.DisplayValue);
    }
}
