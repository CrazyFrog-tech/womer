using Android.App;
using Android.Content;
using Android.OS;

namespace womer.Platforms.Android.Services;

[Service(Enabled = true, Exported = false, ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
public sealed class WorkoutTimerForegroundService : Service
{
    private const string NotificationChannelId = "womer.timer.channel";
    private const string NotificationChannelName = "Workout Timer";
    private const int NotificationId = 21001;

    private const string ActionUpdate = "womer.action.UPDATE_TIMER_NOTIFICATION";
    private const string ActionStop = "womer.action.STOP_TIMER_NOTIFICATION";
    private const string ExtraPhase = "womer.extra.PHASE";
    private const string ExtraTime = "womer.extra.TIME";
    private const string ExtraSet = "womer.extra.SET";

    private PowerManager.WakeLock? _wakeLock;

    public override IBinder? OnBind(Intent? intent) => null;

    public static void StartOrUpdate(string phase, string time, string setLabel)
    {
        var context = global::Android.App.Application.Context;
        using var intent = new Intent(context, typeof(WorkoutTimerForegroundService));
        intent.SetAction(ActionUpdate);
        intent.PutExtra(ExtraPhase, phase);
        intent.PutExtra(ExtraTime, time);
        intent.PutExtra(ExtraSet, setLabel);

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
            context.StartForegroundService(intent);
        else
            context.StartService(intent);
    }

    public static void Stop()
    {
        var context = global::Android.App.Application.Context;
        using var intent = new Intent(context, typeof(WorkoutTimerForegroundService));
        intent.SetAction(ActionStop);
        context.StartService(intent);
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        EnsureNotificationChannel();

        if (intent?.Action == ActionStop)
        {
            ReleaseWakeLock();

            if (OperatingSystem.IsAndroidVersionAtLeast(24))
            {
                StopForeground(StopForegroundFlags.Remove);
            }
            else
            {
#pragma warning disable CA1416
                StopForeground(true);
#pragma warning restore CA1416
            }
            StopSelf();
            return StartCommandResult.NotSticky;
        }

        string phase = intent?.GetStringExtra(ExtraPhase) ?? "WORK";
        string time = intent?.GetStringExtra(ExtraTime) ?? "00:00";
        string set = intent?.GetStringExtra(ExtraSet) ?? "1/1";

        Notification notification = BuildNotification(phase, time, set);
        StartForeground(NotificationId, notification);
        AcquireWakeLock();

        return StartCommandResult.NotSticky;
    }

    public override void OnDestroy()
    {
        ReleaseWakeLock();
        base.OnDestroy();
    }

    private Notification BuildNotification(string phase, string time, string set)
    {
        var launchIntent = new Intent(this, typeof(MainActivity));
        launchIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop | ActivityFlags.NewTask);

        PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            pendingIntentFlags |= PendingIntentFlags.Immutable;

        var pendingIntent = PendingIntent.GetActivity(this, 0, launchIntent, pendingIntentFlags);

        string content = $"{phase} • {time} • Set {set}";

        Notification.Builder builder = OperatingSystem.IsAndroidVersionAtLeast(26)
            ? new Notification.Builder(this, NotificationChannelId)
            : new Notification.Builder(this);

        builder = builder
            .SetContentTitle("Workout timer")
            .SetContentText(content)
            .SetStyle(new Notification.BigTextStyle().BigText(content))
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetOnlyAlertOnce(true)
            .SetOngoing(true)
            .SetContentIntent(pendingIntent)
            .SetCategory(Notification.CategoryProgress)
            .SetVisibility(NotificationVisibility.Public);

        return builder.Build();
    }

    private void EnsureNotificationChannel()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var manager = GetSystemService(NotificationService) as NotificationManager;
        if (manager is null)
            return;

        if (manager.GetNotificationChannel(NotificationChannelId) is not null)
            return;

        var channel = new NotificationChannel(NotificationChannelId, NotificationChannelName, NotificationImportance.Low)
        {
            Description = "Shows workout timer phase and remaining time"
        };

        manager.CreateNotificationChannel(channel);
    }

    private void AcquireWakeLock()
    {
        var powerManager = GetSystemService(PowerService) as PowerManager;
        if (powerManager is null)
            return;

        _wakeLock ??= powerManager.NewWakeLock(WakeLockFlags.Partial, $"{PackageName}:WorkoutTimerWakeLock");
        if (_wakeLock.IsHeld)
            return;

        _wakeLock.Acquire();
    }

    private void ReleaseWakeLock()
    {
        if (_wakeLock?.IsHeld != true)
            return;

        _wakeLock.Release();
    }
}
