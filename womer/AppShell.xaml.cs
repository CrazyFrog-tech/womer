namespace womer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void SettingsToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }
    }
}
