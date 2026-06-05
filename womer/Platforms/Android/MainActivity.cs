using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace womer
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private static WeakReference<MainActivity>? _currentActivity;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _currentActivity = new WeakReference<MainActivity>(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            _currentActivity = new WeakReference<MainActivity>(this);
        }

        protected override void OnDestroy()
        {
            if (_currentActivity?.TryGetTarget(out var activity) == true && ReferenceEquals(activity, this))
                _currentActivity = null;

            base.OnDestroy();
        }

        public static void SetTimerLockScreenMode(bool enabled)
        {
            if (_currentActivity?.TryGetTarget(out var activity) != true)
                return;

            activity.RunOnUiThread(() => activity.ApplyTimerLockScreenMode(enabled));
        }

        private void ApplyTimerLockScreenMode(bool enabled)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(27))
            {
                SetShowWhenLocked(enabled);
                SetTurnScreenOn(enabled);
            }
            else
            {
#pragma warning disable CA1416
                if (enabled)
                {
                    Window?.AddFlags(WindowManagerFlags.ShowWhenLocked | WindowManagerFlags.TurnScreenOn);
                }
                else
                {
                    Window?.ClearFlags(WindowManagerFlags.ShowWhenLocked | WindowManagerFlags.TurnScreenOn);
                }
#pragma warning restore CA1416
            }

            if (!enabled || !OperatingSystem.IsAndroidVersionAtLeast(26))
                return;

            var keyguardManager = GetSystemService(KeyguardService) as KeyguardManager;
            keyguardManager?.RequestDismissKeyguard(this, null);
        }
    }
}
