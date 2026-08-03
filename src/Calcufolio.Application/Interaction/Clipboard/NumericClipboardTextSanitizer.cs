using System.Globalization;

namespace Calcufolio.Application.Interaction.Clipboard;

public sealed class NumericClipboardTextSanitizer : IClipboardTextSanitizer
{
    public string? Sanitize(
        string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        string normalizedText = text
            .Trim()
            .Replace(
                '−',
                '-')
            .Replace(
                ',',
                '.');

        if (!double.TryParse(
                normalizedText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double value) ||
            !double.IsFinite(value))
        {
            return null;
        }

        return normalizedText;
    }
}
