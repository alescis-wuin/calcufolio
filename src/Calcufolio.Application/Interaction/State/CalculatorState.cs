using Calcufolio.Application.Calculations;

namespace Calcufolio.Application.Interaction.State;

public sealed record CalculatorState
{
    public static CalculatorState Initial { get; } = new();

    public string Expression { get; init; } = string.Empty;

    public string DisplayValue { get; init; } = "0";

    public PendingBinaryOperation? PendingOperation { get; init; }

    public bool HasError { get; init; }

    public bool ReplaceDisplayOnNextInput { get; init; }

    public IReadOnlyList<CalculationHistoryEntry> HistoryEntries { get; init; } =
        Array.Empty<CalculationHistoryEntry>();
}
