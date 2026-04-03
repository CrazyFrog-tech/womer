using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using womer.Services;

namespace womer
{
    public partial class MainPage : ContentPage
    {
        private const string InitialPermissionsRequestedKey = "MainPage.InitialPermissionsRequested";

        private readonly IWorkoutService? _workoutService;
        private readonly int _minSeconds = 1;
        private readonly int _minSets = 1;
        private readonly Label? _workSecondsErrorLabel;
        private readonly Label? _restSecondsErrorLabel;
        private readonly Label? _setsErrorLabel;
        private readonly ILogger<MainPage> _logger;
        private bool _isRequestingInitialPermissions;

        public MainPage()
        {
            InitializeComponent();

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                WorkMinutesEntry?.Unfocus();
                WorkSecondsEntry?.Unfocus();
                RestMinutesEntry?.Unfocus();
                RestSecondsEntry?.Unfocus();
                NumberOfSetsEntry?.Unfocus();

                if (WorkMinutesEntry != null)
                {
                    await WorkMinutesEntry.HideSoftInputAsync(System.Threading.CancellationToken.None);
                }
            };
            this.Content.GestureRecognizers.Add(tapGesture);

            _logger = IPlatformApplication.Current?.Services.GetService<ILogger<MainPage>>()
                ?? NullLogger<MainPage>.Instance;

            try {
                _workSecondsErrorLabel = FindByName("WorkSecondsErrorLabel") as Label
                    ?? throw new InvalidOperationException("Work seconds error label not found.");
                _restSecondsErrorLabel = FindByName("RestSecondsErrorLabel") as Label
                    ?? throw new InvalidOperationException("Rest seconds error label not found.");
                _setsErrorLabel = FindByName("SetsErrorLabel") as Label
                    ?? throw new InvalidOperationException("Sets error label not found.");

                if (IPlatformApplication.Current == null)
                    throw new InvalidOperationException("Platform application is not initialized.");

                _workoutService = IPlatformApplication.Current.Services.GetRequiredService<IWorkoutService>();
                if (_workoutService == null)
                    throw new InvalidOperationException("Workout service not available.");
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "Failed to initialize MainPage.");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RequestInitialPermissionsAsync();
        }

        private async Task RequestInitialPermissionsAsync()
        {
            if (_isRequestingInitialPermissions)
                return;

            if (Preferences.Default.Get(InitialPermissionsRequestedKey, false))
                return;

            _isRequestingInitialPermissions = true;

            try
            {
#if ANDROID
                if (OperatingSystem.IsAndroidVersionAtLeast(33))
                {
                    PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                    if (status != PermissionStatus.Granted)
                        await Permissions.RequestAsync<Permissions.PostNotifications>();
                }
#endif
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Unable to request initial app permissions.");
            }
            finally
            {
                Preferences.Default.Set(InitialPermissionsRequestedKey, true);
                _isRequestingInitialPermissions = false;
            }
        }

        private void MinutesEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            UpdateEntryText(entry, NormalizeNumericInput(entry.Text, 2, 59));
        }

        private void SecondsEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            UpdateEntryText(entry, NormalizeNumericInput(entry.Text, 2, 59));
        }

        private void NumberOfSetsEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            UpdateEntryText(entry, NormalizeNumericInput(entry.Text, 2, 99));
        }

        private void TimeEntry_Focused(object sender, FocusEventArgs e)
        {
            if (sender is not Entry entry)
                return;

            if (entry.Text is "00" or "59")
                entry.Text = string.Empty;
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
                ClearValidationErrors();
            }
        }
        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            ClearValidationErrors();

            NormalizeWorkTimeInputs();
            NormalizeRestTimeInputs();

            _workoutService.WorkMinutes = ParseEntryOrDefault(WorkMinutesEntry);
            _workoutService.RestMinutes = ParseEntryOrDefault(RestMinutesEntry);
            _workoutService.RestSeconds = ParseEntryOrDefault(RestSecondsEntry);

            bool hasErrors = !TrySetWorkSeconds() | !TrySetSets();
            if (hasErrors)
                return;

            await Shell.Current.GoToAsync("TimerPage");
        }

        private void NormalizeWorkTimeInputs()
        {
            if (!string.IsNullOrWhiteSpace(WorkSecondsEntry.Text) && string.IsNullOrWhiteSpace(WorkMinutesEntry.Text))
                WorkMinutesEntry.Text = "00";
        }

        private void NormalizeRestTimeInputs()
        {
            if (string.IsNullOrWhiteSpace(RestMinutesEntry.Text) && string.IsNullOrWhiteSpace(RestSecondsEntry.Text))
            {
                RestMinutesEntry.Text = "00";
                RestSecondsEntry.Text = "00";
            }
        }

        private bool TrySetWorkSeconds()
        {
            if ((!TryParseEntry(WorkSecondsEntry, out int seconds) || seconds < _minSeconds) && (!TryParseEntry(WorkMinutesEntry, out int minutes) || minutes < 1))
            {
                ShowError(_workSecondsErrorLabel, $"Seconds must be at least {_minSeconds}.");
                WorkSecondsEntry.Text = string.Empty;
                return false;
            }

            _workoutService.WorkSeconds = seconds;
            return true;
        }

        private bool TrySetSets()
        {
            if (!TryParseEntry(NumberOfSetsEntry, out int sets) || sets < _minSets)
            {
                if(_setsErrorLabel != null)
                ShowError(_setsErrorLabel, $"Sets must be at least {_minSets}.");
                NumberOfSetsEntry.Text = string.Empty;
                return false;
            }

            _workoutService?.Sets = sets;
            return true;
        }

        private static int ParseEntryOrDefault(Entry entry, int fallback = 0)
        {
            return TryParseEntry(entry, out int value) ? value : fallback;
        }

        private static bool TryParseEntry(Entry entry, out int value)
        {
            return int.TryParse(entry.Text, out value);
        }

        private static string NormalizeNumericInput(string? text, int maxLength, int maxValue)
        {
            string digits = new string((text ?? string.Empty).Where(char.IsDigit).ToArray());

            if (digits.Length > maxLength)
                digits = digits.Substring(0, maxLength);

            if (int.TryParse(digits, out int value) && value > maxValue)
                digits = maxValue.ToString();

            return digits;
        }

        private static void UpdateEntryText(Entry entry, string text)
        {
            if (entry.Text != text)
                entry.Text = text;
        }

        private static void ShowError(Label label, string message)
        {
            label.Text = message;
            label.IsVisible = true;
        }

        private void ClearValidationErrors()
        {
            if (_workSecondsErrorLabel != null)
                _workSecondsErrorLabel.IsVisible = false;
            if (_restSecondsErrorLabel != null)
                _restSecondsErrorLabel.IsVisible = false;
            if (_setsErrorLabel != null)
                _setsErrorLabel.IsVisible = false;
        }
    }
}
