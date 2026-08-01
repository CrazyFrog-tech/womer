using Microsoft.Extensions.Logging;
using womer.Application.UseCases;
using womer.Core.Interfaces;
using womer.Core.Models;
using womer.Application.UseCases.WorkoutCollectionUseCases;
using OperationCanceledException = System.OperationCanceledException;



#if ANDROID
using Android.Content;
using Android.OS;
#endif

namespace womer.Presentation.Pages;

[QueryProperty(nameof(CollectionId), "collectionId")]
[QueryProperty(nameof(PlayAll), "playAll")]

public partial class TimerPage : ContentPage
{
    private readonly Color _workBackgroundColor;
	private readonly Color _restBackgroundColor;
	private readonly Color _foregroundColor;
	private readonly Color _prepBackgroundColor = Colors.Red;

	private readonly CountdownRingDrawable _ringDrawable = new();
	private readonly IWorkoutSettings? _workoutService;
	private readonly LoadWorkoutPlanUseCase _loadWorkoutPlanUseCase;
	private readonly ILogger<TimerPage> _logger;
	private readonly ITimerNotificationService _timerNotificationService;
	private readonly ITimerSoundService _timerSoundService;
	private readonly INavigationService _navigationService;
	private CancellationTokenSource? _timerCancellation;
	private bool _timerStarted;
	private bool _isTimerRunning;
	private bool _isPaused;
	private bool _isExitConfirmationVisible;
	private bool _isNavigatingBack;

	private int _totalWorkSeconds;
	private int _totalRestSeconds;
	private int _totalSets;
	private const int PreparationPhaseSeconds = 5;
	private readonly List<WorkoutSession> _workoutSessions = [];

    private readonly ReadWorkoutCollectionUseCase _readWorkoutCollectionUseCase;
	private readonly GetAllWorkoutCollectionsUseCase _getAllWorkoutCollectionsUseCase;
    private readonly LoadWorkoutPlanFromWorkoutCollectionUseCase _loadWorkoutPlanFromCollectionUseCase;

    public long CollectionId { get; set; }
	public bool PlayAll { get; set; }


    public TimerPage(
		IWorkoutSettings workoutService,
		LoadWorkoutPlanUseCase loadWorkoutPlanUseCase,
		GetAllWorkoutCollectionsUseCase getAllWorkoutCollectionsUseCase,
		ReadWorkoutCollectionUseCase readWorkoutCollectionUseCase,
		LoadWorkoutPlanFromWorkoutCollectionUseCase loadWorkoutPlanFromCollectionUseCase,
		ILogger<TimerPage> logger,
		ITimerNotificationService timerNotificationService,
		ITimerSoundService timerSoundService,
		INavigationService navigationService)
	{
		InitializeComponent();
		_workoutService = workoutService ?? throw new ArgumentNullException(nameof(workoutService));
		_loadWorkoutPlanUseCase = loadWorkoutPlanUseCase ?? throw new ArgumentNullException(nameof(loadWorkoutPlanUseCase));
		_getAllWorkoutCollectionsUseCase = getAllWorkoutCollectionsUseCase ?? throw new ArgumentNullException(nameof(getAllWorkoutCollectionsUseCase));
		_readWorkoutCollectionUseCase = readWorkoutCollectionUseCase ?? throw new ArgumentNullException(nameof(readWorkoutCollectionUseCase));
		_loadWorkoutPlanFromCollectionUseCase = loadWorkoutPlanFromCollectionUseCase ?? throw new ArgumentNullException(nameof(loadWorkoutPlanFromCollectionUseCase));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_timerNotificationService = timerNotificationService ?? throw new ArgumentNullException(nameof(timerNotificationService));
		_timerSoundService = timerSoundService ?? throw new ArgumentNullException(nameof(timerSoundService));
		_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

		if (FindByName("PauseButton") is Button pauseButton)
            pauseButton.Clicked += PauseButton_Clicked;

		if (FindByName("CancelButton") is Button cancelButton)
			cancelButton.Clicked += CancelButton_Clicked;

		_workBackgroundColor = GetColor("Primary", Color.FromArgb("#E1D816"));
		_restBackgroundColor = GetColor("RestGreen", Color.FromArgb("#66BB6A"));
		_foregroundColor = GetColor("Black", Colors.Black);

		_ringDrawable.RingColor = _foregroundColor;
		RingView.Drawable = _ringDrawable;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (Shell.Current is not null)
			Shell.Current.Navigating += OnShellNavigating;

		if (_timerStarted)
		{
			if (_isTimerRunning)
			{
				SetKeepScreenOn(true);
				_timerNotificationService.SetLockScreenMode(true);
			}

			return;
		}

		_timerStarted = true;
		await StartWorkoutTimerAsync();
	}

	protected override void OnDisappearing()
	{
		if (Shell.Current is not null)
			Shell.Current.Navigating -= OnShellNavigating;

		SetKeepScreenOn(false);

        base.OnDisappearing();
    }

	private async Task StartWorkoutTimerAsync()
	{
		if (!await TryLoadWorkoutSessionsAsync())
		{
			await DisplayAlertAsync("Timer", "Invalid workout values.", "OK");
			await NavigateBackAsync();
			return;
		}

		try
		{
			await _timerNotificationService.EnsurePermissionAsync();
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "Unable to request notification permission.");
		}

		_timerCancellation = new CancellationTokenSource();
		CancellationToken cancellationToken = _timerCancellation.Token;
		_isTimerRunning = true;
		_isPaused = false;
		SetKeepScreenOn(true);
		_timerNotificationService.SetLockScreenMode(true);
		SetPauseButtonText("Pause");

		try
		{
			foreach (WorkoutSession workoutSession in _workoutSessions)
			{
				cancellationToken.ThrowIfCancellationRequested();
				ApplyWorkoutSession(workoutSession);
				_timerNotificationService.StartOrUpdate("READY", 1, _totalSets, TimeSpan.FromSeconds(PreparationPhaseSeconds));

				await RunPreparationPhaseAsync(PreparationPhaseSeconds, cancellationToken);

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
			}

            PlayWorkoutCompleteCue();
            await PlayConfettiAsync();

			await DisplayAlertAsync("Workout", PlayAll ? "All collections complete." : "Workout complete.", "OK");
			await NavigateBackAsync();
		}
        catch (OperationCanceledException)
		{
			_logger.LogInformation("Workout timer canceled.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error while running workout timer.");
			await DisplayAlertAsync("Timer", "An unexpected error occurred.", "OK");
			await NavigateBackAsync();
		}
		finally
		{
			_isTimerRunning = false;
			SetKeepScreenOn(false);
			_timerNotificationService.SetLockScreenMode(false);
			_timerCancellation?.Dispose();
			_timerCancellation = null;
			_timerNotificationService.Stop();
		}
	}

	private void SetKeepScreenOn(bool keepScreenOn)
	{
		try
		{
			DeviceDisplay.Current.KeepScreenOn = keepScreenOn;
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "Unable to change keep screen on setting.");
		}
	}

	private async Task ConfirmExitWhileTimerRunningAsync()
	{
		_isExitConfirmationVisible = true;

		try
		{
			bool shouldStopTimer = await DisplayAlertAsync(
				"Stop timer?",
				"The timer is still running. Stop it and go back?",
				"Stop",
				"Stay");

			if (!shouldStopTimer || _isNavigatingBack)
				return;

			_timerCancellation?.Cancel();
			await NavigateBackAsync();
		}
		finally
		{
			_isExitConfirmationVisible = false;
		}
	}

	private Task NavigateBackAsync()
	{
		_isNavigatingBack = true;
		return _navigationService.GoToAsync("..");
	}

	private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
	{
		if (!_isTimerRunning || _isNavigatingBack || _isExitConfirmationVisible || !e.CanCancel)
			return;

		if (e.Source is not ShellNavigationSource.Pop && e.Source is not ShellNavigationSource.PopToRoot)
			return;

		ShellNavigatingDeferral navigationDeferral = e.GetDeferral();

		try
		{
			e.Cancel();
			await ConfirmExitWhileTimerRunningAsync();
		}
		finally
		{
			navigationDeferral.Complete();
		}
	}

	private static Color GetColor(string key, Color fallback)
	{
		if (Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue(key, out object? value) == true && value is Color color)
			return color;

		return fallback;
	}

	private async Task<bool> TryLoadWorkoutSessionsAsync()
	{
		_workoutSessions.Clear();

		if (PlayAll)
		{
			IEnumerable<WorkoutCollection> collections = await _getAllWorkoutCollectionsUseCase.ExecuteAsync();
			foreach (WorkoutCollection collection in collections)
			{
				WorkoutPlan collectionPlan = _loadWorkoutPlanFromCollectionUseCase.Execute(collection);
				if (!collectionPlan.IsValid)
				{
					_logger.LogWarning("WorkoutCollection {Id} has invalid values and will be skipped.", collection.Id);
					continue;
				}

				_workoutSessions.Add(new WorkoutSession(collection.Name, collectionPlan));
			}

			return _workoutSessions.Count > 0;
		}

		if (CollectionId > 0)
		{
			WorkoutCollection? workoutCollection = await _readWorkoutCollectionUseCase.ExecuteAsync(CollectionId);
			if (workoutCollection is null)
			{
				_logger.LogWarning("WorkoutCollection {Id} not found.", CollectionId);
				return false;
			}

			WorkoutPlan collectionPlan = _loadWorkoutPlanFromCollectionUseCase.Execute(workoutCollection);
			if (!collectionPlan.IsValid)
				return false;

			_workoutSessions.Add(new WorkoutSession(workoutCollection.Name, collectionPlan));
			return true;
		}

		if (_workoutService is null)
			return false;

		WorkoutPlan workoutPlan = _loadWorkoutPlanUseCase.Execute();
		if (!workoutPlan.IsValid)
			return false;

		_workoutSessions.Add(new WorkoutSession(null, workoutPlan));
		return true;
	}

	private void ApplyWorkoutSession(WorkoutSession workoutSession)
	{
		_totalWorkSeconds = workoutSession.Plan.TotalWorkSeconds;
		_totalRestSeconds = workoutSession.Plan.TotalRestSeconds;
		_totalSets = workoutSession.Plan.TotalSets;
		SetCollectionName(workoutSession.CollectionName);
	}

	private void SetCollectionName(string? collectionName)
	{
		if (string.IsNullOrWhiteSpace(collectionName))
		{
			CollectionNameLabel.Text = string.Empty;
			CollectionNameLabel.IsVisible = false;
			return;
		}

		CollectionNameLabel.Text = collectionName;
		CollectionNameLabel.IsVisible = true;
	}

	private async Task RunPreparationPhaseAsync(int durationSeconds, CancellationToken cancellationToken)
	{
		await RunCountdownRingAsync(
			"READY",
			_prepBackgroundColor,
			currentSet: 1,
			totalSets: _totalSets,
			durationSeconds,
			cancellationToken,
			onSecondChanged: secondsRemaining =>
			{
				if (secondsRemaining > 0)
					PlayTickCue();
			});

		PlayStartingWhistleCue();
	}

	private async Task RunPhaseAsync(int currentSet, int totalSets, int durationSeconds, bool isWorkPhase, CancellationToken cancellationToken)
	{
		const int phaseSwitchPauseMs = 500;
		string phaseText = isWorkPhase ? "WORK" : "REST";
		Color backgroundColor = isWorkPhase ? _workBackgroundColor : _restBackgroundColor;

		await RunCountdownRingAsync(
			phaseText,
			backgroundColor,
			currentSet,
			totalSets,
			durationSeconds,
			cancellationToken,
			onSecondChanged: secondsRemaining =>
			{
				if (secondsRemaining is > 0 and < 5)
					PlayTickCue();
			});

		bool hasAnotherPhase = isWorkPhase
			? currentSet < totalSets && _totalRestSeconds > 0
			: currentSet < totalSets;

		if (hasAnotherPhase)
		{
			PlayPhaseSwitchCue();
			await Task.Delay(phaseSwitchPauseMs, cancellationToken);
		}
	}

	private async Task RunCountdownRingAsync(
		string phaseText,
		Color backgroundColor,
		int currentSet,
		int totalSets,
		int durationSeconds,
		CancellationToken cancellationToken,
		Action<int>? onSecondChanged = null)
	{
		if (durationSeconds <= 0)
		{
			UpdateTimerDisplay(phaseText, backgroundColor, currentSet, totalSets, 0, 0);
			return;
		}

		var stopwatch = System.Diagnostics.Stopwatch.StartNew();
		int lastDisplayedSecond = -1;

		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (_isPaused)
			{
				stopwatch.Stop();
				await WaitWhilePausedAsync(cancellationToken);
				stopwatch.Start();
			}

			double remainingSeconds = Math.Max(0, durationSeconds - stopwatch.Elapsed.TotalSeconds);
			double progress = remainingSeconds / durationSeconds;
			int displaySeconds = remainingSeconds <= 0 ? 0 : (int)Math.Ceiling(remainingSeconds);

			if (displaySeconds != lastDisplayedSecond)
			{
				lastDisplayedSecond = displaySeconds;
				onSecondChanged?.Invoke(displaySeconds);
			}

			UpdateTimerDisplay(phaseText, backgroundColor, currentSet, totalSets, displaySeconds, progress);

			if (remainingSeconds <= 0)
				break;

			await Task.Delay(16, cancellationToken);
		}

		UpdateTimerDisplay(phaseText, backgroundColor, currentSet, totalSets, 0, 0);
	}

	private async Task WaitWhilePausedAsync(CancellationToken cancellationToken)
	{
		while (_isPaused)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await Task.Delay(120, cancellationToken);
		}
	}
	

	private void UpdateTimerDisplay(string phaseText, Color backgroundColor, int currentSet, int totalSets, int secondsRemaining, double progress)
	{
		BackgroundColor = backgroundColor;
		PhaseLabel.Text = phaseText;
		SetLabel.Text = $"{currentSet}/{totalSets}";

		TimeSpan time = TimeSpan.FromSeconds(secondsRemaining);
		TimeLabel.Text = $"{time.Minutes:D2}:{time.Seconds:D2}";

		_ringDrawable.RingProgress = Math.Clamp(progress, 0, 1);
		_ringDrawable.RingColor = _foregroundColor;
		RingView.Invalidate();
		_timerNotificationService.StartOrUpdate(phaseText, currentSet, totalSets, TimeSpan.FromSeconds(secondsRemaining));
	}

	private void SetPauseButtonText(string text)
	{
		if (FindByName("PauseButton") is Button pauseButton)
			pauseButton.Text = text;
	}

	private async void CancelButton_Clicked(object? sender, EventArgs e)
	{
        if (_isNavigatingBack || _isExitConfirmationVisible)
            return;

        if (_isTimerRunning)
        {
            await ConfirmExitWhileTimerRunningAsync();
            return;
        }

        await NavigateBackAsync();
    }

	private void PauseButton_Clicked(object? sender, EventArgs e)
	{
		if (!_isTimerRunning || _isNavigatingBack)
			return;

		_isPaused = !_isPaused;
		SetPauseButtonText(_isPaused ? "Resume" : "Pause");

		if (_isPaused)
			_timerNotificationService.StartOrUpdate("PAUSED", 1, 1, TimeSpan.TryParseExact(TimeLabel.Text ?? "00:00", "mm\\:ss", null, out var pausedTime) ? pausedTime : TimeSpan.Zero);
		else
			_timerNotificationService.StartOrUpdate(PhaseLabel.Text ?? "WORK", 1, 1, TimeSpan.TryParseExact(TimeLabel.Text ?? "00:00", "mm\\:ss", null, out var runningTime) ? runningTime : TimeSpan.Zero);
	}

    private void PlayTickCue()
    {
		_timerSoundService.PlayTick();
    }

	private void PlayStartingWhistleCue()
	{
		_timerSoundService.PlayStartingWhistle();
		Vibrate(500);
	}

    private void PlayPhaseSwitchCue()
    {
		_timerSoundService.PlayPhaseSwitch();
        Vibrate(1000);
    }

    private void PlayWorkoutCompleteCue()
    {
		_timerSoundService.PlayWorkoutComplete();
        Vibrate(1800);
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

            animations[animationIndex++] = piece.TranslateToAsync(xDrift, yDrop, duration, Easing.CubicIn);
            animations[animationIndex++] = piece.RotateToAsync(Random.Shared.Next(-360, 361), duration, Easing.Linear);
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

	private sealed record WorkoutSession(string? CollectionName, WorkoutPlan Plan);

	private sealed class CountdownRingDrawable : IDrawable
	{
		private const float RingThickness = 28f;

		public double RingProgress { get; set; } = 1;
		public Color RingColor { get; set; } = Colors.Black;

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			float size = Math.Min(dirtyRect.Width, dirtyRect.Height) - RingThickness;
			if (size <= 0)
				return;

			float x = (dirtyRect.Width - size) / 2f;
			float y = (dirtyRect.Height - size) / 2f;

			canvas.StrokeSize = RingThickness;
			canvas.StrokeLineCap = LineCap.Round;

			canvas.StrokeColor = Color.FromRgba(0, 0, 0, 60);
			canvas.DrawEllipse(x, y, size, size);

			double progress = Math.Clamp(RingProgress, 0, 1);

			if (progress <= 0)
				return;

			canvas.StrokeColor = RingColor;

			// DrawArc with a full 360° sweep often fails to render on some platforms;
			// draw a complete ellipse instead when the ring is essentially full.
			if (progress >= 0.999)
			{
				canvas.DrawEllipse(x, y, size, size);
				return;
			}

			float startAngle = -90f;
			float endAngle = startAngle + (float)(360.0 * progress);
			canvas.DrawArc(x, y, size, size, startAngle, endAngle, true, false);
		}
	}
}
