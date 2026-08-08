using womer.Core.Interfaces;
using womer.Application.UseCases;

namespace womer.Presentation.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SetVolumeUseCase _setVolumeUseCase;
    private readonly GetInitialVolumeUseCase _getInitialVolumeUseCase;
    private readonly INavigationService _navigationService;
    private bool _isInitializingVolume;


    public SettingsPage(
        SetVolumeUseCase setVolumeUseCase,
        GetInitialVolumeUseCase getInitialVolumeUseCase,
        INavigationService navigationService)
    {
        InitializeComponent();
        _setVolumeUseCase = setVolumeUseCase ?? throw new ArgumentNullException(nameof(setVolumeUseCase));
        _getInitialVolumeUseCase = getInitialVolumeUseCase ?? throw new ArgumentNullException(nameof(getInitialVolumeUseCase));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

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

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await _navigationService.GoToAsync("//MainPage");
    }
}