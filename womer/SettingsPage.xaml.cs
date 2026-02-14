using Microsoft.Maui.Storage;

namespace womer;

public partial class SettingsPage : ContentPage
{
    private const string VolumePreferenceKey = "Volume";
    private const string BuyMeCoffeeUrl = "https://buymeacoffee.com/mohamadsolodev";


    public SettingsPage()
    {
        InitializeComponent();

        var volume = Preferences.Default.Get(VolumePreferenceKey, 1.0);
        VolumeSlider.Value = volume;
        UpdateVolumeLabel(volume);
    }

    private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        Preferences.Default.Set(VolumePreferenceKey, e.NewValue);
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