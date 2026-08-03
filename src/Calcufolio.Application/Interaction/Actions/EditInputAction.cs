using Calcufolio.Application.Interaction.Editor.Actions;

namespace Calcufolio.Application.Interaction.Actions;

public sealed record EditInputAction : CalculatorAction
{
    public EditInputAction(
        EditorAction editorAction)
    {
        ArgumentNullException.ThrowIfNull(editorAction);

        EditorAction = editorAction;
    }

    public EditorAction EditorAction { get; }
}
