using Avalonia.Input;
using Calcufolio.Presentation.Input;

namespace Calcufolio.Presentation.Tests.Input;

public sealed class CalculatorKeyFeedbackMapperTests
{
    [Theory]
    [InlineData("0", "0")]
    [InlineData("4", "4")]
    [InlineData("9", "9")]
    public void MapTextMapsDigits(
        string text,
        string expectedKey)
    {
        Assert.Equal(
            expectedKey,
            CalculatorKeyFeedbackMapper.MapText(text));
    }

    [Theory]
    [InlineData(".", ".")]
    [InlineData(",", ".")]
    [InlineData("+", "+")]
    [InlineData("-", "−")]
    [InlineData("*", "×")]
    [InlineData("/", "÷")]
    [InlineData("=", "=")]
    public void MapTextMapsVisibleOperations(
        string text,
        string expectedKey)
    {
        Assert.Equal(
            expectedKey,
            CalculatorKeyFeedbackMapper.MapText(text));
    }

    [Theory]
    [InlineData(Key.NumPad0, "0")]
    [InlineData(Key.NumPad5, "5")]
    [InlineData(Key.NumPad9, "9")]
    [InlineData(Key.Decimal, ".")]
    [InlineData(Key.Add, "+")]
    [InlineData(Key.Subtract, "−")]
    [InlineData(Key.Multiply, "×")]
    [InlineData(Key.Divide, "÷")]
    [InlineData(Key.Enter, "=")]
    [InlineData(Key.Back, "⌫")]
    [InlineData(Key.Escape, "AC")]
    public void MapKeyMapsVisibleCalculatorControls(
        Key key,
        string expectedKey)
    {
        Assert.Equal(
            expectedKey,
            CalculatorKeyFeedbackMapper.MapKey(
                key,
                KeyModifiers.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("x")]
    public void MapTextIgnoresUnsupportedText(
        string? text)
    {
        Assert.Null(
            CalculatorKeyFeedbackMapper.MapText(text));
    }

    [Theory]
    [InlineData(Key.Delete)]
    [InlineData(Key.Left)]
    [InlineData(Key.F1)]
    public void MapKeyIgnoresControlsWithoutVisibleButton(
        Key key)
    {
        Assert.Null(
            CalculatorKeyFeedbackMapper.MapKey(
                key,
                KeyModifiers.None));
    }

    [Theory]
    [InlineData(Key.C)]
    [InlineData(Key.V)]
    [InlineData(Key.X)]
    public void MapKeyIgnoresClipboardShortcuts(
        Key key)
    {
        Assert.Null(
            CalculatorKeyFeedbackMapper.MapKey(
                key,
                KeyModifiers.Control));
    }
}
