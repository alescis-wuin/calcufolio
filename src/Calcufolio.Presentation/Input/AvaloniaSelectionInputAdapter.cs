using Avalonia.Controls;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.Editor.Actions;

namespace Calcufolio.Presentation.Input;

public sealed class AvaloniaSelectionInputAdapter
{
    private readonly ICalculatorController _controller;

    public AvaloniaSelectionInputAdapter(
        ICalculatorController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        _controller = controller;
    }

    public void Synchronize(
        TextBox textBox)
    {
        ArgumentNullException.ThrowIfNull(textBox);

        int textLength =
            textBox.Text?.Length ?? 0;

        TextSelectionSnapshot selection =
            TextSelectionSnapshot.Create(
                textLength,
                textBox.SelectionStart,
                textBox.SelectionEnd,
                textBox.CaretIndex);

        _controller.Dispatch(
            new EditInputAction(
                new SetSelectionEditorAction(
                    selection.AnchorIndex,
                    selection.CaretIndex)));
    }
}
