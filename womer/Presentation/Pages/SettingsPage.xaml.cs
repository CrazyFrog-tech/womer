using womer.Core.Interfaces;
using womer.Application.UseCases;

namespace womer;

public partial class SettingsPage : ContentPage
{
    private const string BuyMeCoffeeUrl = "https://buymeacoffee.com/mohamadsolodev";
    private readonly SetVolumeUseCase _setVolumeUseCase;
    private readonly GetInitialVolumeUseCase _getInitialVolumeUseCase;
    private bool _isInitializingVolume;



    public SettingsPage(SetVolumeUseCase setVolumeUseCase, GetInitialVolumeUseCase getInitialVolumeUseCase)
    {
        InitializeComponent();
        _setVolumeUseCase = setVolumeUseCase ?? throw new ArgumentNullException(nameof(setVolumeUseCase));
        _getInitialVolumeUseCase = getInitialVolumeUseCase ?? throw new ArgumentNullException(nameof(getInitialVolumeUseCase));

        double initialVolume = _getInitialVolumeUseCase.Execute();

        _isInitializingVolume = true;
        VolumeSlider.Value = initialVolume;
        _isInitializingVolume = false;

        _setVolumeUseCase.Execute(initialVolume);
        UpdateVolumeLabel(initialVolume);
    }

    private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isInitializingVolume)
            return;

        _setVolumeUseCase.Execute(e.NewValue);
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