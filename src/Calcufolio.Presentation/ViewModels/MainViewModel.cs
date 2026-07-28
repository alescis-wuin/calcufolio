using CommunityToolkit.Mvvm.ComponentModel;

namespace Calcufolio.Presentation.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    public string Expression { get; } = "sin(45°) + √81 × 2";

    public string DisplayValue { get; } = "18.7071";

    public string AngleMode { get; } = "DEG";

    public string MemoryStatus { get; } = "M  0";

    public IReadOnlyList<CalculationHistoryEntryViewModel> HistoryEntries { get; } =
    [
        new("(18 + 6) ÷ 3", "8"),
        new("√144 + 2³", "20"),
        new("cos(60°) × 100", "50"),
        new("ln(e⁴)", "4"),
    ];

    [ObservableProperty]
    public partial bool IsStartupToastVisible { get; set; } = true;

    public async Task InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(4));

        IsStartupToastVisible = false;
    }
}
