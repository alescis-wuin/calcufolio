namespace Calcufolio.Application.Interaction.Editor.State;

public sealed record EditorState
{
    public EditorState(
        string text,
        int caretIndex,
        int selectionAnchorIndex)
    {
        ArgumentNullException.ThrowIfNull(text);

        ValidateIndex(
            caretIndex,
            text.Length,
            nameof(caretIndex));

        ValidateIndex(
            selectionAnchorIndex,
            text.Length,
            nameof(selectionAnchorIndex));

        Text = text;
        CaretIndex = caretIndex;
        SelectionAnchorIndex = selectionAnchorIndex;
    }

    public static EditorState Empty { get; } =
        FromText(string.Empty);

    public string Text { get; }

    public int CaretIndex { get; }

    public int SelectionAnchorIndex { get; }

    public int SelectionStart =>
        Math.Min(
            CaretIndex,
            SelectionAnchorIndex);

    public int SelectionEnd =>
        Math.Max(
            CaretIndex,
            SelectionAnchorIndex);

    public int SelectionLength =>
        SelectionEnd - SelectionStart;

    public bool HasSelection =>
        SelectionLength > 0;

    public string SelectedText =>
        HasSelection
            ? Text.Substring(
                SelectionStart,
                SelectionLength)
            : string.Empty;

    public static EditorState FromText(
        string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new EditorState(
            text,
            text.Length,
            text.Length);
    }

    private static void ValidateIndex(
        int index,
        int textLength,
        string parameterName)
    {
        if (index < 0 ||
            index > textLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                index,
                "The editor index must be within the text boundaries.");
        }
    }
}
