using Calcufolio.Application.Interaction.State;

namespace Calcufolio.Application.Tests.Interaction.State;

public sealed class CalculatorStateStoreTests
{
    [Fact]
    public void DefaultConstructorUsesInitialState()
    {
        CalculatorStateStore store = new();

        Assert.Same(
            CalculatorState.Initial,
            store.Current);
    }

    [Fact]
    public void ConstructorUsesProvidedInitialState()
    {
        CalculatorState initialState =
            CalculatorState.Initial with
            {
                DisplayValue = "42",
            };

        CalculatorStateStore store = new(initialState);

        Assert.Same(
            initialState,
            store.Current);
    }

    [Fact]
    public void ReplacePublishesNewState()
    {
        CalculatorStateStore store = new();
        CalculatorState replacement =
            CalculatorState.Initial with
            {
                DisplayValue = "7",
            };

        CalculatorState? publishedState = null;

        store.StateChanged += (_, eventArgs) =>
        {
            publishedState = eventArgs.State;
        };

        store.Replace(replacement);

        Assert.Same(
            replacement,
            store.Current);

        Assert.Same(
            replacement,
            publishedState);
    }

    [Fact]
    public void ReplaceDoesNotPublishEquivalentState()
    {
        CalculatorStateStore store = new();
        int publicationCount = 0;

        store.StateChanged += (_, _) =>
        {
            publicationCount++;
        };

        store.Replace(CalculatorState.Initial);

        Assert.Equal(
            0,
            publicationCount);
    }

    [Fact]
    public void ConstructorRejectsMissingInitialState()
    {
        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => new CalculatorStateStore(null!));

        Assert.Equal(
            "initialState",
            exception.ParamName);
    }

    [Fact]
    public void ReplaceRejectsMissingState()
    {
        CalculatorStateStore store = new();

        ArgumentNullException exception =
            Assert.Throws<ArgumentNullException>(
                () => store.Replace(null!));

        Assert.Equal(
            "state",
            exception.ParamName);
    }
}
