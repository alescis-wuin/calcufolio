namespace Calcufolio.Application.Interaction.Editor.Actions;

public sealed record InsertTextEditorAction : EditorAction
{
    public InsertTextEditorAction(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
        {
            throw new ArgumentException(
                "The inserted text must not be empty.",
                nameof(text));
        }

        Text = text;
    }

    public string Text { get; }
}
