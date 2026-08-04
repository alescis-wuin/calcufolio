namespace Calcufolio.Application.Interaction.Editor.Actions;

public sealed record MoveCaretEditorAction(
    int Offset,
    bool ExtendSelection) : EditorAction;
