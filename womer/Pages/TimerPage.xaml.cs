using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using womer.Services;

namespace womer;

public partial class TimerPage : ContentPage
{
	private readonly Color _workBackgroundColor;
	private readonly Color _restBackgroundColor;
	private readonly Color _foregroundColor;

	private readonly CountdownRingDrawable _ringDrawable = new();
	private readonly IWorkoutService? _workoutService;
	private readonly ILogger<TimerPage> _logger;
	private CancellationTokenSource? _timerCancellation;
	private bool _timerStarted;

	private int _totalWorkSeconds;
	private int _totalRestSeconds;
	private int _totalSets;

	public TimerPage()
	{
		InitializeComponent();
		_logger = IPlatformApplication.Current?.Services.GetService<ILogger<TimerPage>>()
			?? NullLogger<TimerPage>.Instance;

		_workBackgroundColor = GetColor("Primary", Color.FromArgb("#E1D816"));
		_restBackgroundColor = GetColor("RestGreen", Color.FromArgb("#66BB6A"));
		_foregroundColor = GetColor("Black", Colors.Black);

		_ringDrawable.RingColor = _foregroundColor;
		RingView.Drawable = _ringDrawable;
		_workoutService = IPlatformApplication.Current?.Services.GetService<IWorkoutService>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (_timerStarted)
			return;

		_timerStarted = true;
		await StartWorkoutTimerAsync();
	}

	protected override void OnDisappearing()
	{
		_timerCancellation?.Cancel();
		_timerCancellation?.Dispose();
		_timerCancellation = null;
		base.OnDisappearing();
	}

	private async Task StartWorkoutTimerAsync()
	{
		if (!TryLoadWorkoutValues())
		{
			await DisplayAlertAsync("Timer", "Invalid workout values.", "OK");
			await Shell.Current.GoToAsync("..");
			return;
		}

		_timerCancellation = new CancellationTokenSource();
		CancellationToken cancellationToken = _timerCancellation.Token;

		try
		{
			for (int set = 1; set <= _totalSets; set++)
			{
				await RunPhaseAsync(set, _totalSets, _totalWorkSeconds, isWorkPhase: true, cancellationToken);

				if (cancellationToken.IsCancellationRequested)
					return;

				bool isLastSet = set == _totalSets;
				if (!isLastSet && _totalRestSeconds > 0)
					await RunPhaseAsync(set, _totalSets, _totalRestSeconds, isWorkPhase: false, cancellationToken);

				if (cancellationToken.IsCancellationRequested)
					return;
			}

			await DisplayAlert("Workout", "Workout complete.", "OK");
			await Shell.Current.GoToAsync("..");
		}
		catch (TaskCanceledException)
		{
			_logger.LogInformation("Workout timer canceled.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while running workout timer.");
			await DisplayAlertAsync("Timer", "An unexpected error occurred.", "OK");
			await Shell.Current.GoToAsync("..");
		}
	}

	private static Color GetColor(string key, Color fallback)
	{
		if (Application.Current?.Resources.TryGetValue(key, out object? value) == true && value is Color color)
			return color;

		return fallback;
	}

	private bool TryLoadWorkoutValues()
	{
		if (_workoutService is null)
			return false;

		_totalWorkSeconds = (_workoutService.WorkMinutes * 60) + _workoutService.WorkSeconds;
		_totalRestSeconds = (_workoutService.RestMinutes * 60) + _workoutService.RestSeconds;
		_totalSets = Math.Max(1, _workoutService.Sets);

		return _totalWorkSeconds > 0;
	}

	private async Task RunPhaseAsync(int currentSet, int totalSets, int durationSeconds, bool isWorkPhase, CancellationToken cancellationToken)
	{
		for (int secondsRemaining = durationSeconds; secondsRemaining >= 0; secondsRemaining--)
		{
			cancellationToken.ThrowIfCancellationRequested();

			double progress = durationSeconds == 0
				? 0
				: (double)secondsRemaining / durationSeconds;

			UpdateTimerDisplay(currentSet, totalSets, secondsRemaining, progress, isWorkPhase);

			if (secondsRemaining == 0)
				break;

			await Task.Delay(1000, cancellationToken);
		}
	}

	private void UpdateTimerDisplay(int currentSet, int totalSets, int secondsRemaining, double progress, bool isWorkPhase)
	{
		BackgroundColor = isWorkPhase ? _workBackgroundColor : _restBackgroundColor;
		PhaseLabel.Text = isWorkPhase ? "WORK" : "REST";
		SetLabel.Text = $"{currentSet}/{totalSets}";

		TimeSpan time = TimeSpan.FromSeconds(secondsRemaining);
		TimeLabel.Text = $"{time.Minutes:D2}:{time.Seconds:D2}";

		_ringDrawable.RingProgress = Math.Clamp(progress, 0, 1);
		_ringDrawable.RingColor = _foregroundColor;
		RingView.Invalidate();
	}

	private sealed class CountdownRingDrawable : IDrawable
	{
		private const float RingThickness = 18f;

		public double RingProgress { get; set; } = 1;
		public Color RingColor { get; set; } = Colors.Black;

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			float size = Math.Min(dirtyRect.Width, dirtyRect.Height) - RingThickness;
			if (size <= 0)
				return;

			float x = (dirtyRect.Width - size) / 2;
			float y = (dirtyRect.Height - size) / 2;

			canvas.StrokeSize = RingThickness;
			canvas.StrokeLineCap = LineCap.Round;

			canvas.StrokeColor = Color.FromRgba(0, 0, 0, 60);
			canvas.DrawEllipse(x, y, size, size);

			if (RingProgress <= 0)
				return;

			float startAngle = -90;
			float endAngle = startAngle + (float)(360 * Math.Clamp(RingProgress, 0, 1));

			canvas.StrokeColor = RingColor;
			canvas.DrawArc(x, y, size, size, startAngle, endAngle, true, false);
		}
	}
}
