using Calcufolio.Application.Interaction.Clipboard;

namespace Calcufolio.Application.Tests.Interaction.Clipboard;

public sealed class NumericClipboardTextSanitizerTests
{
    private readonly NumericClipboardTextSanitizer _sanitizer = new();

    [Theory]
    [InlineData("12", "12")]
    [InlineData(" 12.5 ", "12.5")]
    [InlineData("12,5", "12.5")]
    [InlineData("−12,5", "-12.5")]
    [InlineData("1e3", "1e3")]
    public void SanitizeNormalizesFiniteNumericText(
        string text,
        string expectedText)
    {
        Assert.Equal(
            expectedText,
            _sanitizer.Sanitize(text));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("2 + 3")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("1.2.3")]
    public void SanitizeRejectsUnsupportedText(
        string? text)
    {
        Assert.Null(
            _sanitizer.Sanitize(text));
    }
}
