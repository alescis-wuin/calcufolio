namespace Calcufolio.Application.Interaction.Editor.Actions;

public sealed record MoveCaretToEndEditorAction(
    bool ExtendSelection) : EditorAction;
