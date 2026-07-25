using womer.Presentation.Pages;
using womer.Core.Interfaces;

namespace womer
{
    public partial class AppShell : Shell
    {
        private readonly INavigationService _navigationService;

        public AppShell(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
            Routing.RegisterRoute("TimerPage", typeof(TimerPage));
            Routing.RegisterRoute("EditCollectionPage", typeof(EditCollectionPage));
            Routing.RegisterRoute("CollectionsPage", typeof(CollectionsPage));
            Navigated += OnNavigated;
        }

        private void OnNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            var location = e.Current?.Location?.ToString() ?? string.Empty;
            SettingsToolbarItem.IsEnabled = !location.Contains("Settingspage", StringComparison.OrdinalIgnoreCase) && !location.Contains("TimerPage", StringComparison.OrdinalIgnoreCase);

        }

        private async void SettingsToolbarItem_Clicked(object sender, EventArgs e)
        {
            await _navigationService.GoToAsync("SettingsPage");
        }
    }
}
