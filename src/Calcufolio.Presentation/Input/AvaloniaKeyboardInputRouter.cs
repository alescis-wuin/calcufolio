using Avalonia.Input;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Controller;

namespace Calcufolio.Presentation.Input;

public sealed class AvaloniaKeyboardInputRouter
{
    private readonly ICalculatorController _controller;

    public AvaloniaKeyboardInputRouter(
        ICalculatorController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        _controller = controller;
    }

    public bool RouteText(
        string? text)
    {
        return Dispatch(
            CalculatorKeyboardInputMapper.MapText(text));
    }

    public bool RouteKey(
        Key key,
        KeyModifiers modifiers)
    {
        return Dispatch(
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
