using Avalonia.Controls;
using Avalonia.Threading;

namespace Calcufolio.Presentation.Input;

public sealed class KeyboardButtonFeedbackController
{
    private static readonly TimeSpan _feedbackDuration =
        TimeSpan.FromMilliseconds(130);

    private readonly Dictionary<string, FeedbackEntry> _entries;

    public KeyboardButtonFeedbackController(
        IReadOnlyDictionary<string, Button> buttons)
    {
        ArgumentNullException.ThrowIfNull(buttons);

        Dictionary<string, FeedbackEntry> entries =
            new(StringComparer.Ordinal);

        foreach ((string key, Button button) in buttons)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentNullException.ThrowIfNull(button);

            DispatcherTimer timer = new()
            {
                Interval = _feedbackDuration,
            };

            FeedbackEntry entry =
                new(
                    button,
                    timer);

            timer.Tick += (_, _) =>
            {
                timer.Stop();
                button.Classes.Remove(
                    "keyboard-pressed");
            };

            entries.Add(
                key,
                entry);
        }

        _entries = entries;
    }

    public void Flash(
        string? key)
    {
        if (key is null ||
            !_entries.TryGetValue(
                key,
                out FeedbackEntry? entry))
        {
            return;
        }

        entry.Timer.Stop();

        if (!entry.Button.Classes.Contains(
                "keyboard-pressed"))
        {
            entry.Button.Classes.Add(
                "keyboard-pressed");
        }

        entry.Timer.Start();
    }

    private sealed record FeedbackEntry(
        Button Button,
        DispatcherTimer Timer);
}
