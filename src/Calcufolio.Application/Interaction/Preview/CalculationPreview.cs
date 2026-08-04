namespace Calcufolio.Application.Interaction.Preview;

public sealed record CalculationPreview(
    string Expression,
    string Result)
{
    public string DisplayText =>
        $"{Expression} {Result}";
}
