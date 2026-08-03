namespace Calcufolio.Application.Interaction.Clipboard;

public interface ICalculatorClipboardController
{
    ValueTask CopyAsync(
        CancellationToken cancellationToken = default);

    ValueTask PasteAsync(
        CancellationToken cancellationToken = default);
}
