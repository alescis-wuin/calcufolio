namespace Calcufolio.Application.Interaction.Clipboard;

public interface IClipboardPort
{
    ValueTask<string?> ReadTextAsync(
        CancellationToken cancellationToken = default);

    ValueTask WriteTextAsync(
        string text,
        CancellationToken cancellationToken = default);
}
