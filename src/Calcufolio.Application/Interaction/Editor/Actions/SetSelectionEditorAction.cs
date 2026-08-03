namespace Calcufolio.Application.Interaction.Editor.Actions;

public sealed record SetSelectionEditorAction(
    int AnchorIndex,
    int CaretIndex) : EditorAction;
