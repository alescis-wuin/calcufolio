using Avalonia.Input;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Clipboard;
using Calcufolio.Application.Interaction.Controller;

namespace Calcufolio.Presentation.Input;

public sealed class AvaloniaKeyboardInputRouter
{
    private readonly ICalculatorClipboardController _clipboardController;
    private readonly ICalculatorController _controller;

    public AvaloniaKeyboardInputRouter(
        ICalculatorController controller,
        ICalculatorClipboardController clipboardController)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(clipboardController);

        _controller = controller;
        _clipboardController = clipboardController;
    }

    public static bool CanRouteKey(
        Key key,
        KeyModifiers modifiers)
    {
        return CalculatorKeyboardInputMapper.IsCopyShortcut(
                key,
                modifiers) ||
            CalculatorKeyboardInputMapper.IsCutShortcut(
                key,
                modifiers) ||
            CalculatorKeyboardInputMapper.IsPasteShortcut(
                key,
                modifiers) ||
            CalculatorKeyboardInputMapper.MapKey(
                key,
                modifiers) is not null;
    }

    public bool RouteText(
        string? text)
    {
        return Dispatch(
            CalculatorKeyboardInputMapper.MapText(text));
    }

    public async ValueTask RouteKeyAsync(
        Key key,
        KeyModifiers modifiers,
        CancellationToken cancellationToken = default)
    {
        if (CalculatorKeyboardInputMapper.IsCopyShortcut(
                key,
                modifiers))
        {
            await _clipboardController.CopyAsync(
                cancellationToken);

            return;
        }

        if (CalculatorKeyboardInputMapper.IsCutShortcut(
                key,
                modifiers))
        {
            await _clipboardController.CutAsync(
                cancellationToken);

            return;
        }

        if (CalculatorKeyboardInputMapper.IsPasteShortcut(
                key,
                modifiers))
        {
            await _clipboardController.PasteAsync(
                cancellationToken);

            return;
        }

        _ = Dispatch(
            CalculatorKeyboardInputMapper.MapKey(
                key,
                modifiers));
    }

    private bool Dispatch(
        CalculatorAction? action)
    {
        if (action is null)
        {
            return false;
        }

        _controller.Dispatch(action);

        return true;
    }
}
