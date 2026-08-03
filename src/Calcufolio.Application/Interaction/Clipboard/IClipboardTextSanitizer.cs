namespace Calcufolio.Application.Interaction.Clipboard;

public interface IClipboardTextSanitizer
{
    string? Sanitize(string? text);
}
