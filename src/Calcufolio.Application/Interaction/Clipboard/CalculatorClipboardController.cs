using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.Editor.Actions;
using Calcufolio.Application.Interaction.Editor.State;
using Calcufolio.Application.Interaction.State;

namespace Calcufolio.Application.Interaction.Clipboard;

public sealed class CalculatorClipboardController : ICalculatorClipboardController
{
    private readonly ICalculatorController _calculatorController;
    private readonly IClipboardPort _clipboardPort;
    private readonly IClipboardTextSanitizer _clipboardTextSanitizer;
    private readonly ICalculatorStateStore _stateStore;

    public CalculatorClipboardController(
        ICalculatorController calculatorController,
        ICalculatorStateStore stateStore,
        IClipboardPort clipboardPort,
        IClipboardTextSanitizer clipboardTextSanitizer)
    {
        ArgumentNullException.ThrowIfNull(calculatorController);
        ArgumentNullException.ThrowIfNull(stateStore);
        ArgumentNullException.ThrowIfNull(clipboardPort);
        ArgumentNullException.ThrowIfNull(clipboardTextSanitizer);

        _calculatorController = calculatorController;
        _stateStore = stateStore;
        _clipboardPort = clipboardPort;
        _clipboardTextSanitizer = clipboardTextSanitizer;
    }

    public async ValueTask CopyAsync(
        CancellationToken cancellationToken = default)
    {
        CalculatorState state =
            _stateStore.Current;

        string text = state.Editor.HasSelection
            ? state.Editor.SelectedText
            : state.DisplayValue;

        if (text.Length == 0)
        {
            return;
        }

        await _clipboardPort.WriteTextAsync(
            text,
            cancellationToken);
    }

    public async ValueTask CutAsync(
        CancellationToken cancellationToken = default)
    {
        EditorState editor =
            _stateStore.Current.Editor;

        if (!editor.HasSelection)
        {
            return;
        }

        await _clipboardPort.WriteTextAsync(
            editor.SelectedText,
            cancellationToken);

        if (_stateStore.Current.Editor != editor)
        {
            return;
        }

        _calculatorController.Dispatch(
            new EditInputAction(
                new BackspaceEditorAction()));
    }

    public async ValueTask PasteAsync(
        CancellationToken cancellationToken = default)
    {
        string? clipboardText =
            await _clipboardPort.ReadTextAsync(
                cancellationToken);

        string? sanitizedText =
            _clipboardTextSanitizer.Sanitize(
                clipboardText);

        if (sanitizedText is null)
        {
            return;
        }

        _calculatorController.Dispatch(
            new EditInputAction(
                new InsertTextEditorAction(
                    sanitizedText)));
    }
}
