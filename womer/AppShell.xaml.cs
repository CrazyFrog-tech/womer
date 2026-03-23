namespace womer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
            Routing.RegisterRoute("TimerPage", typeof(TimerPage));
            Navigated += OnNavigated;
        }

        private void OnNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            var location = e.Current?.Location?.ToString() ?? string.Empty;
            SettingsToolbarItem.IsEnabled = !location.Contains("Settingspage", StringComparison.OrdinalIgnoreCase) && !location.Contains("TimerPage", StringComparison.OrdinalIgnoreCase);

        }

        private async void SettingsToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }
    }
}
