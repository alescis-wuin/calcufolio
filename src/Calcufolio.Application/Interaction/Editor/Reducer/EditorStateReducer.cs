using Calcufolio.Application.Interaction.Editor.Actions;
using Calcufolio.Application.Interaction.Editor.State;

namespace Calcufolio.Application.Interaction.Editor.Reducer;

public sealed class EditorStateReducer : IEditorStateReducer
{
    public EditorState Reduce(
        EditorState state,
        EditorAction action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        return action switch
        {
            InsertTextEditorAction insertText =>
                InsertText(
                    state,
                    insertText.Text),

            BackspaceEditorAction =>
                Backspace(state),

            DeleteForwardEditorAction =>
                DeleteForward(state),

            MoveCaretEditorAction moveCaret =>
                MoveCaret(
                    state,
                    moveCaret.Offset,
                    moveCaret.ExtendSelection),

            MoveCaretToStartEditorAction moveToStart =>
                MoveCaretTo(
                    state,
                    0,
                    moveToStart.ExtendSelection),

            MoveCaretToEndEditorAction moveToEnd =>
                MoveCaretTo(
                    state,
                    state.Text.Length,
                    moveToEnd.ExtendSelection),

            SetSelectionEditorAction setSelection =>
                SetSelection(
                    state,
                    setSelection.AnchorIndex,
                    setSelection.CaretIndex),

            SelectAllEditorAction =>
                SelectAll(state),

            _ => throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The editor action is not supported."),
        };
    }

    private static EditorState InsertText(
        EditorState state,
        string insertedText)
    {
        int insertionIndex =
            state.SelectionStart;

        string updatedText = state.Text
            .Remove(
                insertionIndex,
                state.SelectionLength)
            .Insert(
                insertionIndex,
                insertedText);

        int updatedCaretIndex =
            insertionIndex + insertedText.Length;

        return new EditorState(
            updatedText,
            updatedCaretIndex,
            updatedCaretIndex);
    }

    private static EditorState Backspace(
        EditorState state)
    {
        if (state.HasSelection)
        {
            return DeleteSelection(state);
        }

        if (state.CaretIndex == 0)
        {
            return state;
        }

        int deletionIndex =
            state.CaretIndex - 1;

        string updatedText =
            state.Text.Remove(
                deletionIndex,
                1);

        return new EditorState(
            updatedText,
            deletionIndex,
            deletionIndex);
    }

    private static EditorState DeleteForward(
        EditorState state)
    {
        if (state.HasSelection)
        {
            return DeleteSelection(state);
        }

        if (state.CaretIndex == state.Text.Length)
        {
            return state;
        }

        string updatedText =
            state.Text.Remove(
                state.CaretIndex,
                1);

        return new EditorState(
            updatedText,
            state.CaretIndex,
            state.CaretIndex);
    }

    private static EditorState DeleteSelection(
        EditorState state)
    {
        string updatedText =
            state.Text.Remove(
                state.SelectionStart,
                state.SelectionLength);

        return new EditorState(
            updatedText,
            state.SelectionStart,
            state.SelectionStart);
    }

    private static EditorState MoveCaret(
        EditorState state,
        int offset,
        bool extendSelection)
    {
        int targetIndex;

        if (!extendSelection &&
            state.HasSelection)
        {
            targetIndex = offset switch
            {
                < 0 => state.SelectionStart,
                > 0 => state.SelectionEnd,
                _ => state.CaretIndex,
            };
        }
        else
        {
            long requestedIndex =
                (long)state.CaretIndex + offset;

            targetIndex = (int)Math.Clamp(
                requestedIndex,
                0L,
                state.Text.Length);
        }

        return MoveCaretTo(
            state,
            targetIndex,
            extendSelection);
    }

    private static EditorState MoveCaretTo(
        EditorState state,
        int targetIndex,
        bool extendSelection)
    {
        int clampedIndex = Math.Clamp(
            targetIndex,
            0,
            state.Text.Length);

        int anchorIndex = extendSelection
            ? state.SelectionAnchorIndex
            : clampedIndex;

        return new EditorState(
            state.Text,
            clampedIndex,
            anchorIndex);
    }

    private static EditorState SetSelection(
        EditorState state,
        int anchorIndex,
        int caretIndex)
    {
        int clampedAnchorIndex = Math.Clamp(
            anchorIndex,
            0,
            state.Text.Length);

        int clampedCaretIndex = Math.Clamp(
            caretIndex,
            0,
            state.Text.Length);

        return new EditorState(
            state.Text,
            clampedCaretIndex,
            clampedAnchorIndex);
    }

    private static EditorState SelectAll(
        EditorState state)
    {
        return new EditorState(
            state.Text,
            state.Text.Length,
            0);
    }
}
