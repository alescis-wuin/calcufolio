using Calcufolio.Application.Calculations;
using Calcufolio.Domain.Calculations;
using Calcufolio.Presentation.ViewModels;

namespace Calcufolio.Presentation.Tests.ViewModels;

internal static class MainViewModelTestFactory
{
    public static MainViewModel Create()
    {
        return new MainViewModel(
            new CalculatorSession(
                new CalculationEngine()));
    }
}
