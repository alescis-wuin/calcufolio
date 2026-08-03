namespace Calcufolio.Application.Interaction.Editor.Actions;

public sealed record MoveCaretToStartEditorAction(
    bool ExtendSelection) : EditorAction;
