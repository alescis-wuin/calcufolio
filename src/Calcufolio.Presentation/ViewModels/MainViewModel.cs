using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calcufolio.Presentation.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Expression { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DisplayValue { get; set; } = "0";

    public string AngleMode { get; } = "DEG";

    public string MemoryStatus { get; } = "M  0";

    public IReadOnlyList<CalculationHistoryEntryViewModel> HistoryEntries { get; } = [];

    [ObservableProperty]
    public partial bool IsStartupToastVisible { get; set; } = true;

    [RelayCommand]
    private void AppendDigit(string digit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(digit);

        if (digit.Length != 1 ||
            !char.IsAsciiDigit(digit[0]))
        {
            throw new ArgumentException(
                "The digit must contain exactly one ASCII numeric character.",
                nameof(digit));
        }

        DisplayValue = DisplayValue == "0"
            ? digit
            : $"{DisplayValue}{digit}";
    }

    [RelayCommand]
    private void AppendDecimalSeparator()
    {
        if (!DisplayValue.Contains(
                '.',
                StringComparison.Ordinal))
        {
            DisplayValue = $"{DisplayValue}.";
        }
    }

    [RelayCommand]
    private void Clear()
    {
        Expression = string.Empty;
        DisplayValue = "0";
    }

    public async Task InitializeAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(4));

        IsStartupToastVisible = false;
    }
}
