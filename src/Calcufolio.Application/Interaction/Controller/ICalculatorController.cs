using Calcufolio.Application.Interaction.Actions;

namespace Calcufolio.Application.Interaction.Controller;

public interface ICalculatorController
{
    void Dispatch(CalculatorAction action);
}
