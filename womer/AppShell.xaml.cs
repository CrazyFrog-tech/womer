namespace womer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("settingsPage", typeof(SettingsPage));

        }

        private async void SettingsToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("settingsPage");
        }
    }
}
