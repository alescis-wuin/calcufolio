using Calcufolio.Application.Interaction.State;

namespace Calcufolio.Application.Interaction.Preview;

public interface ICalculationPreviewService
{
    CalculationPreview? Create(
        CalculatorState state);
}
