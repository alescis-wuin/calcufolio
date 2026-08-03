using Avalonia.Input;
using Calcufolio.Application.Interaction.Actions;
using Calcufolio.Application.Interaction.Editor.Actions;
using Calcufolio.Presentation.Input;

namespace Calcufolio.Presentation.Tests.Input;

public sealed class CalculatorKeyboardInputMapperTests
{
    [Theory]
    [InlineData("0", "0")]
    [InlineData("7", "7")]
    [InlineData("9", "9")]
    public void MapTextMapsDigits(
        string text,
        string expectedDigit)
    {
        AppendDigitAction action =
            Assert.IsType<AppendDigitAction>(
                CalculatorKeyboardInputMapper.MapText(text));

        Assert.Equal(
            expectedDigit,
            action.Digit);
    }

    [Theory]
    [InlineData(".")]
    [InlineData(",")]
    public void MapTextMapsDecimalSeparators(
        string text)
    {
        Assert.IsType<AppendDecimalSeparatorAction>(
            CalculatorKeyboardInputMapper.MapText(text));
    }

    [Theory]
    [InlineData("+", "+")]
    [InlineData("-", "−")]
    [InlineData("*", "×")]
    [InlineData("/", "÷")]
    public void MapTextMapsOperators(
        string text,
        string expectedSymbol)
    {
        SelectOperatorAction action =
            Assert.IsType<SelectOperatorAction>(
                CalculatorKeyboardInputMapper.MapText(text));

        Assert.Equal(
            expectedSymbol,
            action.OperatorSymbol);
    }

    [Fact]
    public void MapTextMapsEquals()
    {
        Assert.IsType<EvaluateAction>(
            CalculatorKeyboardInputMapper.MapText("="));
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
            CalculatorKeyboardInputMapper.MapText(text));
    }

    [Fact]
    public void MapKeyMapsBackspace()
    {
        EditInputAction action =
            Assert.IsType<EditInputAction>(
                CalculatorKeyboardInputMapper.MapKey(
                    Key.Back,
                    KeyModifiers.None));

        Assert.IsType<BackspaceEditorAction>(
            action.EditorAction);
    }

    [Fact]
    public void MapKeyMapsDeleteForward()
    {
        EditInputAction action =
            Assert.IsType<EditInputAction>(
                CalculatorKeyboardInputMapper.MapKey(
                    Key.Delete,
                    KeyModifiers.None));

        Assert.IsType<DeleteForwardEditorAction>(
            action.EditorAction);
    }

    [Fact]
    public void MapKeyMapsShiftArrowToExtendedSelection()
    {
        EditInputAction action =
            Assert.IsType<EditInputAction>(
                CalculatorKeyboardInputMapper.MapKey(
                    Key.Left,
                    KeyModifiers.Shift));

        MoveCaretEditorAction editorAction =
            Assert.IsType<MoveCaretEditorAction>(
                action.EditorAction);

        Assert.Equal(
            -1,
            editorAction.Offset);

        Assert.True(
            editorAction.ExtendSelection);
    }

    [Theory]
    [InlineData(KeyModifiers.Control)]
    [InlineData(KeyModifiers.Meta)]
    public void MapKeyMapsPrimarySelectAllShortcut(
        KeyModifiers modifiers)
    {
        EditInputAction action =
            Assert.IsType<EditInputAction>(
                CalculatorKeyboardInputMapper.MapKey(
                    Key.A,
                    modifiers));

        Assert.IsType<SelectAllEditorAction>(
            action.EditorAction);
    }

    [Theory]
    [InlineData(KeyModifiers.Control)]
    [InlineData(KeyModifiers.Meta)]
    public void IsCopyShortcutRecognizesPrimaryModifier(
        KeyModifiers modifiers)
    {
        Assert.True(
            CalculatorKeyboardInputMapper.IsCopyShortcut(
                Key.C,
                modifiers));
    }

    [Theory]
    [InlineData(KeyModifiers.Control)]
    [InlineData(KeyModifiers.Meta)]
    public void IsPasteShortcutRecognizesPrimaryModifier(
        KeyModifiers modifiers)
    {
        Assert.True(
            CalculatorKeyboardInputMapper.IsPasteShortcut(
                Key.V,
                modifiers));
    }

    [Fact]
    public void ClipboardShortcutsRejectMissingPrimaryModifier()
    {
        Assert.False(
            CalculatorKeyboardInputMapper.IsCopyShortcut(
                Key.C,
                KeyModifiers.None));

        Assert.False(
            CalculatorKeyboardInputMapper.IsPasteShortcut(
                Key.V,
                KeyModifiers.None));
    }

    [Fact]
    public void MapKeyMapsEnterToEvaluation()
    {
        Assert.IsType<EvaluateAction>(
            CalculatorKeyboardInputMapper.MapKey(
                Key.Enter,
                KeyModifiers.None));
    }

    [Fact]
    public void MapKeyMapsEscapeToClear()
    {
        Assert.IsType<ClearAction>(
            CalculatorKeyboardInputMapper.MapKey(
                Key.Escape,
                KeyModifiers.None));
    }

    [Fact]
    public void MapKeyIgnoresUnsupportedKey()
    {
        Assert.Null(
            CalculatorKeyboardInputMapper.MapKey(
                Key.F1,
                KeyModifiers.None));
    }
}
