using Calcufolio.Application.Calculations;
using Calcufolio.Application.Interaction.Editor.State;

namespace Calcufolio.Application.Interaction.State;

public sealed record CalculatorState
{
    public static CalculatorState Initial { get; } = new();

    public string Expression { get; init; } = string.Empty;

    public EditorState Editor { get; init; } =
        EditorState.FromText("0");

    public string DisplayValue =>
        Editor.Text;

    public PendingBinaryOperation? PendingOperation { get; init; }

    public bool HasError { get; init; }

    public bool ReplaceDisplayOnNextInput { get; init; }

    public IReadOnlyList<CalculationHistoryEntry> HistoryEntries { get; init; } =
        Array.Empty<CalculationHistoryEntry>();
}
