using System;
using System.Timers;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Timer = System.Timers.Timer;

namespace womer;

public partial class WorkoutTimer : ContentPage
{
    private Timer? _timer;
    private TimeSpan _remaining = TimeSpan.Zero;
    private TimeSpan _initial = TimeSpan.FromSeconds(60);
    private bool _isRunning = false;

    private TimeSpan? _firstInitial;
    private bool _firstInitialCaptured = false;

    public WorkoutTimer()
    {
        InitializeComponent();
        UpdateTimeLabel(_initial);

        _initial = TimeSpan.FromSeconds(parseSecondsEntryOrDefault(WorkTimeLabel.Text));
        _remaining = _initial;

        _timer = new Timer(1000) { AutoReset = true };
        _timer.Elapsed += Timer_Elapsed;
    }

    private void StartButton_Clicked(object? sender, EventArgs e)
    {

        
        if (!_isRunning)
        {
            if (_remaining == TimeSpan.Zero)
            {
                _initial = TimeSpan.FromSeconds(parseSecondsEntryOrDefault(WorkTimeLabel.Text));
                _remaining = _initial;
            }

            if (!_firstInitialCaptured)
            {
                _firstInitial = _remaining;
                _firstInitialCaptured = true;
            }

            _timer?.Start();
            _isRunning = true;
        }
    }

    private void PauseButton_Clicked(object? sender, EventArgs e)
    {
        if (_isRunning)
        {
            _timer?.Stop();
            _isRunning = false;
        }
    }

    private void ResetButton_Clicked(object? sender, EventArgs e)
    {
        _timer?.Stop();
        _isRunning = false;
        _remaining = _firstInitial.GetValueOrDefault(TimeSpan.Zero);
        UpdateTimeLabel(_remaining);
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (_remaining > TimeSpan.Zero)
        {
            _remaining = _remaining - TimeSpan.FromSeconds(1);
            MainThread.BeginInvokeOnMainThread(() => UpdateTimeLabel(_remaining));
        }

        if (_remaining <= TimeSpan.Zero)
        {
            _timer?.Stop();
            _isRunning = false;
            MainThread.BeginInvokeOnMainThread(() => UpdateTimeLabel(TimeSpan.Zero));
            // Optional: add sound or vibration here.
        }
    }

    private void UpdateTimeLabel(TimeSpan ts)
    {
        WorkTimeLabel.Text = ts.ToString(@"mm\:ss");
    }

    private int parseSecondsEntryOrDefault(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 60;

        text = text.Trim();

        if (text.Contains(":"))
        {
            var parts = text.Split(':');
            if (parts.Length == 2
                && int.TryParse(parts[0], out var minutes)
                && int.TryParse(parts[1], out var seconds))
            {
                if (minutes < 0) minutes = 0;
                if (seconds < 0) seconds = 0;
                minutes += seconds / 60;
                seconds = seconds % 60;
                var total = minutes * 60 + seconds;
                return total > 0 ? total : 60;
            }

            return 60;
        }

        if (int.TryParse(text, out var s) && s > 0)
            return s;

        return 60;
    }

    private void workTimeLabel_Unfocused(object? sender, FocusEventArgs e)
    {
        _initial = TimeSpan.FromSeconds(parseSecondsEntryOrDefault(WorkTimeLabel.Text));
        _remaining = _initial;
        UpdateTimeLabel(_remaining);
        _timer?.Stop();
        _isRunning = false;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        _isRunning = false;
    }
}