using Calcufolio.Application.Interaction.Controller;
using Calcufolio.Application.Interaction.State;
using Calcufolio.Domain.Calculations;
using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Tests.ViewModels;

internal static class MainViewModelTestFactory
{
    public static MainViewModel Create(
        CalculatorState? initialState = null)
    {
        ICalculatorStateStore stateStore = initialState is null
            ? new CalculatorStateStore()
            : new CalculatorStateStore(initialState);

        ICalculatorController controller =
            new CalculatorController(
                new CalculationEngine(),
                stateStore);

        return new MainViewModel(
            controller,
            stateStore);
    }
}
