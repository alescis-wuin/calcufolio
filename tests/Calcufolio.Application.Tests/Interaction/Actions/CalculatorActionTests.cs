using Calcufolio.Application.Interaction.Actions;

namespace Calcufolio.Application.Tests.Interaction.Actions;

public sealed class CalculatorActionTests
{
    [Theory]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("x")]
    public void AppendDigitRejectsInvalidInput(
        string digit)
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new AppendDigitAction(digit));

        Assert.Equal(
            "digit",
            exception.ParamName);
    }

    [Fact]
    public void AppendDigitStoresValidatedDigit()
    {
        AppendDigitAction action = new("7");

        Assert.Equal(
            "7",
            action.Digit);
    }

    [Fact]
    public void SelectOperatorRejectsEmptySymbol()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new SelectOperatorAction(string.Empty));

        Assert.Equal(
            "operatorSymbol",
            exception.ParamName);
    }
}
