using Calcufolio.Application.Interaction.Editor.Actions;
using Calcufolio.Application.Interaction.Editor.State;

namespace Calcufolio.Application.Interaction.Editor.Reducer;

public interface IEditorStateReducer
{
    EditorState Reduce(
        EditorState state,
        EditorAction action);
}
