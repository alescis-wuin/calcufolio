namespace Calcufolio.Presentation.Input;

public readonly record struct TextSelectionSnapshot(
    int AnchorIndex,
    int CaretIndex)
{
    public static TextSelectionSnapshot Create(
        int textLength,
        int selectionStart,
        int selectionEnd,
        int caretIndex)
    {
        if (textLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(textLength),
                textLength,
                "The text length must not be negative.");
        }

        int start = Math.Clamp(
            Math.Min(
                selectionStart,
                selectionEnd),
            0,
            textLength);

        int end = Math.Clamp(
            Math.Max(
                selectionStart,
                selectionEnd),
            0,
            textLength);

        int caret = Math.Clamp(
            caretIndex,
            0,
            textLength);

        int anchor = start == end
            ? caret
            : caret <= start
                ? end
                : start;

        return new TextSelectionSnapshot(
            anchor,
            caret);
    }
}
