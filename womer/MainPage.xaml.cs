namespace womer
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }
        private void MinutesEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            string text = entry.Text ?? string.Empty;

            string digits = new string(text.Where(char.IsDigit).ToArray());

            if (digits.Length > 2)
                digits = digits.Substring(0, 2);

            if (int.TryParse(digits, out int mm) && mm > 59)
                digits = "59";

            if (entry.Text != digits)
                entry.Text = digits;
        }

        private void SecondsEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            string text = entry.Text ?? string.Empty;

            string digits = new string(text.Where(char.IsDigit).ToArray());

            if (digits.Length > 2)
                digits = digits.Substring(0, 2);

            if (int.TryParse(digits, out int ss) && ss > 59)
                digits = "59";

            if (entry.Text != digits)
                entry.Text = digits;
        }

        private void NumberOfSetsEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            string text = entry.Text ?? string.Empty;

            string digits = new string(text.Where(char.IsDigit).ToArray());

            if (digits.Length > 2)
                digits = digits.Substring(0, 2);

            if (int.TryParse(digits, out int ss) && ss > 99)
                digits = "99";

            if (entry.Text != digits)
                entry.Text = digits;
        }

        private async void  ClearButton_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlertAsync("Confirm", "Are you sure you want to clear all fields?", "Yes", "No");
            if (confirm)
            {
                WorkMinutesEntry.Text = string.Empty;
                WorkSecondsEntry.Text = string.Empty;
                RestMinutesEntry.Text = string.Empty;
                RestSecondsEntry.Text = string.Empty;
                NumberOfSetsEntry.Text = string.Empty;
            }
        }
        private void StartButton_Clicked(object sender, EventArgs e)
        {
            DisplayAlertAsync("Start", "Start button clicked.", "OK");
        }





    }
}
