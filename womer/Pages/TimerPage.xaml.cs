using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using womer.Services;
#if ANDROID
using Android.Media;
using Android.Content;
using Android.OS;
#endif

namespace womer;

public partial class TimerPage : ContentPage
{
#if ANDROID
    private readonly ToneGenerator _toneGenerator = new(Android.Media.Stream.Notification, 70);
#endif

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

#if ANDROID
        _toneGenerator.Release();
        _toneGenerator.Dispose();
#endif

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
            PlayWorkoutCompleteCue();
            await PlayConfettiAsync();

            await DisplayAlertAsync("Workout", "Workout complete.", "OK");
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
		const double ringOffset = 0.95;
        const int phaseSwitchPauseMs = 500;
        for (int secondsRemaining = durationSeconds; secondsRemaining >= 0; secondsRemaining--)
		{
			cancellationToken.ThrowIfCancellationRequested();
			double adjustedRemaining = secondsRemaining - ringOffset;

            double progress = durationSeconds == 0
                ? 0
				: (double)adjustedRemaining / durationSeconds;

            UpdateTimerDisplay(currentSet, totalSets, secondsRemaining, progress, isWorkPhase);

            if (secondsRemaining > 0)
                PlayTickCue();

            if (secondsRemaining == 0)
            {
                bool hasAnotherPhase = isWorkPhase
                       ? currentSet < totalSets && _totalRestSeconds > 0
                       : currentSet < totalSets;

                if (hasAnotherPhase)
                {
                    PlayPhaseSwitchCue();
                    await Task.Delay(phaseSwitchPauseMs, cancellationToken);
                }

                break;
            }

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
    private void PlayTickCue()
    {
#if ANDROID
        _toneGenerator.StartTone(Tone.SupBusy, 90);
#endif
    }

    private void PlayPhaseSwitchCue()
    {
#if ANDROID
        _toneGenerator.StartTone(Tone.CdmaAbbrReorder, 400);
#endif
        Vibrate(150);
    }

    private void PlayWorkoutCompleteCue()
    {
#if ANDROID
        _toneGenerator.StartTone(Tone.CdmaAlertCallGuard, 900);
#endif
        Vibrate(200);
    }

    private async Task PlayConfettiAsync()
    {
        if (ConfettiLayer is null)
            return;

        if (ConfettiLayer.Width <= 0 || ConfettiLayer.Height <= 0)
            await Task.Delay(16);

        double width = ConfettiLayer.Width > 0 ? ConfettiLayer.Width : Width;
        double height = ConfettiLayer.Height > 0 ? ConfettiLayer.Height : Height;

        if (width <= 0 || height <= 0)
            return;

        Color[] confettiColors =
        [
            Colors.Yellow,
            Colors.Orange,
            Colors.DeepSkyBlue,
            Colors.HotPink,
            Colors.MediumSpringGreen,
            Colors.White
        ];

        const int pieceCount = 42;
        Task[] animations = new Task[pieceCount * 2];
        int animationIndex = 0;

        ConfettiLayer.Children.Clear();
        ConfettiLayer.IsVisible = true;
        ConfettiLayer.Opacity = 1;

        for (int i = 0; i < pieceCount; i++)
        {
            var piece = new BoxView
            {
                Color = confettiColors[Random.Shared.Next(confettiColors.Length)],
                WidthRequest = Random.Shared.Next(6, 12),
                HeightRequest = Random.Shared.Next(10, 18),
                Rotation = Random.Shared.Next(-45, 45),
                CornerRadius = 2,
                Opacity = 0.95
            };

            double startX = Random.Shared.NextDouble() * Math.Max(10, width - 10);
            double startY = -20 - Random.Shared.Next(0, 120);
            AbsoluteLayout.SetLayoutBounds(piece, new Rect(startX, startY, piece.WidthRequest, piece.HeightRequest));
            ConfettiLayer.Children.Add(piece);

            double xDrift = Random.Shared.Next(-100, 101);
            double yDrop = height + Random.Shared.Next(40, 140);
            uint duration = (uint)Random.Shared.Next(900, 1700);

            animations[animationIndex++] = piece.TranslateTo(xDrift, yDrop, duration, Easing.CubicIn);
            animations[animationIndex++] = piece.RotateTo(Random.Shared.Next(-360, 361), duration, Easing.Linear);
        }

        await Task.WhenAll(animations);
        await ConfettiLayer.FadeToAsync(0, 120);

        ConfettiLayer.Children.Clear();
        ConfettiLayer.Opacity = 1;
        ConfettiLayer.IsVisible = false;
    }

    private void Vibrate(int milliseconds)
    {
        try
        {
#if ANDROID
            int durationMs = Math.Max(1, milliseconds);
            var context = Android.App.Application.Context;

            Vibrator? vibrator = null;

            if (OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                var manager = context.GetSystemService(Context.VibratorManagerService) as VibratorManager;
                vibrator = manager?.DefaultVibrator;
            }
            else
            {
                #pragma warning disable CS0618
                vibrator = context.GetSystemService(Context.VibratorService) as Vibrator;
                #pragma warning restore CS0618
            }

            if (vibrator?.HasVibrator == true)
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(26))
                    vibrator.Vibrate(VibrationEffect.CreateOneShot(durationMs, VibrationEffect.DefaultAmplitude));
                else
                    #pragma warning disable CS0618
                    vibrator.Vibrate(durationMs);
                    #pragma warning restore CS0618

                return;
            }
#endif

            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(milliseconds));
        }
        catch (FeatureNotSupportedException)
        {
            _logger.LogDebug("Vibration is not supported on this device.");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to vibrate device.");
        }
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

            double progress = Math.Clamp(RingProgress, 0, 1);

			if (progress <= 0)
				return;

            float startAngle = -90;
            float endAngle = startAngle + (float)(360 * progress);

            canvas.StrokeColor = RingColor;
            canvas.DrawArc(x, y, size, size, startAngle, endAngle, true, false);
        }
    }
}
