using Microsoft.Maui.Storage;
using womer.Services;
#if ANDROID
using Android.Content;
using Android.Media;
#endif

namespace womer;

public partial class SettingsPage : ContentPage
{
    private const string BuyMeCoffeeUrl = "https://buymeacoffee.com/mohamadsolodev";
    private readonly IWorkoutService _settings;



    public SettingsPage()
    {
        InitializeComponent();

        if (IPlatformApplication.Current == null)
            throw new InvalidOperationException("Platform application is not initialized.");

        _settings = IPlatformApplication.Current.Services.GetRequiredService<IWorkoutService>();
        if (_settings == null)
            throw new InvalidOperationException("Settings service not available.");

        double initialVolume = _settings.Volume;
        if (TryGetSystemVolume(out double systemVolume))
            initialVolume = systemVolume;

        VolumeSlider.Value = initialVolume;
        _settings.Volume = initialVolume;
        UpdateVolumeLabel(initialVolume);
    }

    private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        _settings.Volume = e.NewValue;
        SetSystemVolume(e.NewValue);
        UpdateVolumeLabel(e.NewValue);
    }

    private void UpdateVolumeLabel(double volume)
    {
        VolumeValueLabel.Text = $"Volume: {(int)(volume * 100)}%";
    }
    private async void BuyMeCoffeeButton_Clicked(object sender, EventArgs e)
    {
        await Browser.Default.OpenAsync(BuyMeCoffeeUrl, BrowserLaunchMode.SystemPreferred);
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    private static bool TryGetSystemVolume(out double normalizedVolume)
    {
        normalizedVolume = 0;

#if ANDROID
        var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
        if (audioManager == null)
            return false;

        int max = audioManager.GetStreamMaxVolume(Android.Media.Stream.Notification);
        int current = audioManager.GetStreamVolume(Android.Media.Stream.Notification);

        if (max <= 0)
            return false;

        normalizedVolume = (double)current / max;
        return true;
#else
        return false;
#endif
    }

    private static void SetSystemVolume(double normalizedVolume)
    {
#if ANDROID
        var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
        if (audioManager == null)
            return;

        int max = audioManager.GetStreamMaxVolume(Android.Media.Stream.Notification);
        if (max <= 0)
            return;

        int target = (int)Math.Round(Math.Clamp(normalizedVolume, 0, 1) * max);
        audioManager.SetStreamVolume(Android.Media.Stream.Notification, target, VolumeNotificationFlags.PlaySound);
#endif
    }
}