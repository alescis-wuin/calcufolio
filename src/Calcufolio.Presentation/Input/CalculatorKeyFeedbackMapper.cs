using Avalonia.Input;

namespace Calcufolio.Presentation.Input;

public static class CalculatorKeyFeedbackMapper
{
    public static string? MapText(
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
            return character.ToString();
        }

        return character switch
        {
            '.' or ',' => ".",
            '+' => "+",
            '-' or '−' => "−",
            '*' or '×' => "×",
            '/' or '÷' => "÷",
            '=' => "=",
            _ => null,
        };
    }

    public static string? MapKey(
        Key key,
        KeyModifiers modifiers)
    {
        if (HasPrimaryModifier(modifiers))
        {
            return null;
        }

        return key switch
        {
            Key.NumPad0 => "0",
            Key.NumPad1 => "1",
            Key.NumPad2 => "2",
            Key.NumPad3 => "3",
            Key.NumPad4 => "4",
            Key.NumPad5 => "5",
            Key.NumPad6 => "6",
            Key.NumPad7 => "7",
            Key.NumPad8 => "8",
            Key.NumPad9 => "9",
            Key.Decimal => ".",
            Key.Add => "+",
            Key.Subtract => "−",
            Key.Multiply => "×",
            Key.Divide => "÷",
            Key.Enter => "=",
            Key.Back => "⌫",
            Key.Escape => "AC",
            _ => null,
        };
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
