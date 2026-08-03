namespace Calcufolio.Application.Interaction.State;

public sealed class CalculatorStateStore : ICalculatorStateStore
{
    public CalculatorStateStore()
        : this(CalculatorState.Initial)
    {
    }

    public CalculatorStateStore(
        CalculatorState initialState)
    {
        ArgumentNullException.ThrowIfNull(initialState);

        Current = initialState;
    }

    public CalculatorState Current { get; private set; }

    public event EventHandler<CalculatorStateChangedEventArgs>? StateChanged;

    public void Replace(
        CalculatorState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (Current == state)
        {
            return;
        }

        Current = state;

        StateChanged?.Invoke(
            this,
            new CalculatorStateChangedEventArgs(state));
    }
}
