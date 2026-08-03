namespace Calcufolio.Application.Interaction.State;

public sealed class CalculatorStateChangedEventArgs : EventArgs
{
    public CalculatorStateChangedEventArgs(
        CalculatorState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        State = state;
    }

    public CalculatorState State { get; }
}
