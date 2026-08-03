using Avalonia.Input;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Editor.Actions;

namespace Calcufolio.Presentation.Input;

public static class CalculatorKeyboardInputMapper
{
    public static CalculatorAction? MapText(
        string? text)
    {
        if (string.IsNullOrEmpty(text) ||
            text.Length != 1)
        {
            return null;
        }

        char character = text[0];

        if (char.IsAsciiDigit(character))
        {
            return new AppendDigitAction(
                character.ToString());
        }

        return character switch
        {
            '.' or ',' =>
                new AppendDecimalSeparatorAction(),

            '+' =>
                new SelectOperatorAction("+"),

            '-' or '−' =>
                new SelectOperatorAction("−"),

            '*' or '×' =>
                new SelectOperatorAction("×"),

            '/' or '÷' =>
                new SelectOperatorAction("÷"),

            '=' =>
                new EvaluateAction(),

            _ => null,
        };
    }

    public static CalculatorAction? MapKey(
        Key key,
        KeyModifiers modifiers)
    {
        bool extendSelection =
            HasModifier(
                modifiers,
                KeyModifiers.Shift);

        bool primaryModifier =
            HasPrimaryModifier(modifiers);

        if (primaryModifier &&
            key == Key.A)
        {
            return new EditInputAction(
                new SelectAllEditorAction());
        }

        return key switch
        {
            Key.Back =>
                new EditInputAction(
                    new BackspaceEditorAction()),

            Key.Delete =>
                new EditInputAction(
                    new DeleteForwardEditorAction()),

            Key.Left =>
                new EditInputAction(
                    new MoveCaretEditorAction(
                        -1,
                        extendSelection)),

            Key.Right =>
                new EditInputAction(
                    new MoveCaretEditorAction(
                        1,
                        extendSelection)),

            Key.Home =>
                new EditInputAction(
                    new MoveCaretToStartEditorAction(
                        extendSelection)),

            Key.End =>
                new EditInputAction(
                    new MoveCaretToEndEditorAction(
                        extendSelection)),

            Key.Enter =>
                new EvaluateAction(),

            Key.Escape =>
                new ClearAction(),

            Key.Add =>
                new SelectOperatorAction("+"),

            Key.Subtract =>
                new SelectOperatorAction("−"),

            Key.Multiply =>
                new SelectOperatorAction("×"),

            Key.Divide =>
                new SelectOperatorAction("÷"),

            Key.Decimal =>
                new AppendDecimalSeparatorAction(),

            _ => null,
        };
    }

    public static bool IsCopyShortcut(
        Key key,
        KeyModifiers modifiers)
    {
        return key == Key.C &&
            HasPrimaryModifier(modifiers);
    }

    public static bool IsPasteShortcut(
        Key key,
        KeyModifiers modifiers)
    {
        return key == Key.V &&
            HasPrimaryModifier(modifiers);
    }

    private static bool HasPrimaryModifier(
        KeyModifiers modifiers)
    {
        return HasModifier(
                modifiers,
                KeyModifiers.Control) ||
            HasModifier(
                modifiers,
                KeyModifiers.Meta);
    }

    private static bool HasModifier(
        KeyModifiers value,
        KeyModifiers modifier)
    {
        return (value & modifier) == modifier;
    }
}
