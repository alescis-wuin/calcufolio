using Avalonia.Input.Platform;
using Calcufolio.Application.Interaction.Clipboard;

namespace Calcufolio.Presentation.Input;

public sealed class AvaloniaClipboardPort : IClipboardPort
{
    private readonly Func<IClipboard?> _clipboardProvider;

    public AvaloniaClipboardPort(
        Func<IClipboard?> clipboardProvider)
    {
        ArgumentNullException.ThrowIfNull(clipboardProvider);

        _clipboardProvider = clipboardProvider;
    }

    public async ValueTask<string?> ReadTextAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IClipboard? clipboard =
            _clipboardProvider();

        if (clipboard is null)
        {
            return null;
        }

        string? text =
            await clipboard.TryGetTextAsync();

        cancellationToken.ThrowIfCancellationRequested();

        return text;
    }

    public async ValueTask WriteTextAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);

        cancellationToken.ThrowIfCancellationRequested();

        IClipboard? clipboard =
            _clipboardProvider();

        if (clipboard is null)
        {
            return;
        }

        await clipboard.SetTextAsync(text);

        cancellationToken.ThrowIfCancellationRequested();
    }
}
