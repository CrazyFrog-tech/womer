using womer.Core.Interfaces;

#if ANDROID
using womer.Platforms.Android.Services;
#endif

namespace womer.Infrastructure.Notifications
{
    public sealed class TimerNotificationService : ITimerNotificationService
    {
        public async Task EnsurePermissionAsync()
        {
#if ANDROID
            try
            {
                if (!OperatingSystem.IsAndroidVersionAtLeast(33))
                    return;

                var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                if (status != PermissionStatus.Granted)
                    await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
            catch
            {
                // no-op by design
            }
#endif
            await Task.CompletedTask;
        }

        public void StartOrUpdate(string phase, int currentSet, int totalSets, TimeSpan remaining)
        {
#if ANDROID
            WorkoutTimerForegroundService.StartOrUpdate(
                phase,
                remaining.ToString(@"mm\:ss"),
                $"{currentSet}/{totalSets}");
#endif
        }

        public void Stop()
        {
#if ANDROID
            WorkoutTimerForegroundService.Stop();
#endif
        }

        public void SetLockScreenMode(bool enabled)
        {
#if ANDROID
            womer.MainActivity.SetTimerLockScreenMode(enabled);
#endif
        }
    }
}
