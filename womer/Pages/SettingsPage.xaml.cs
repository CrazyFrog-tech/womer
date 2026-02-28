using Microsoft.Maui.Storage;
using womer.Services;

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

        VolumeSlider.Value = _settings.Volume;
        UpdateVolumeLabel(_settings.Volume);
    }

    private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        _settings.Volume = e.NewValue;
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
}