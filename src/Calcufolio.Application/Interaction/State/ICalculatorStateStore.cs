namespace Calcufolio.Application.Interaction.State;

public interface ICalculatorStateStore
{
    CalculatorState Current { get; }

    event EventHandler<CalculatorStateChangedEventArgs>? StateChanged;

    void Replace(CalculatorState state);
}
