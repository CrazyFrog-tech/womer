using Microsoft.Extensions.Logging;
using womer.Application.UseCases.WorkoutCollectionUseCases;
using womer.Core.Interfaces;
using womer.Core.Models;

namespace womer.Presentation.Pages
{
    [QueryProperty(nameof(CollectionId), "collectionId")]
    public partial class EditCollectionPage : ContentPage
    {
        private const string InitialPermissionsRequestedKey = "EditCollectionPage.InitialPermissionsRequested";

        private readonly int _minSeconds = 1;
        private readonly int _minSets = 1;
        private readonly Label? _workSecondsErrorLabel;
        private readonly Label? _restSecondsErrorLabel;
        private readonly Label? _setsErrorLabel;
        private readonly ILogger<EditCollectionPage> _logger;
        private readonly ReadWorkoutCollectionUseCase _readWorkoutCollectionUseCase;
        private readonly SaveWorkoutCollectionUseCase _saveWorkoutCollectionUseCase;
        private readonly INavigationService _navigationService;
        private bool _isRequestingInitialPermissions;
        private bool _isCollectionLoaded;
        private long _collectionId;
        private string CollectionName => CollectionNameEntry?.Text?.Trim() ?? string.Empty;

        public string CollectionId
        {
            set => _collectionId = long.TryParse(value, out long collectionId) ? collectionId : 0;
        }

        public EditCollectionPage(
            ReadWorkoutCollectionUseCase readWorkoutCollectionUseCase,
            SaveWorkoutCollectionUseCase saveWorkoutCollectionUseCase,
            ILogger<EditCollectionPage> logger,
            INavigationService navigationService)
        {
            InitializeComponent();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _readWorkoutCollectionUseCase = readWorkoutCollectionUseCase ?? throw new ArgumentNullException(nameof(readWorkoutCollectionUseCase));
            _saveWorkoutCollectionUseCase = saveWorkoutCollectionUseCase ?? throw new ArgumentNullException(nameof(saveWorkoutCollectionUseCase));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));


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

            try {
                _workSecondsErrorLabel = FindByName("WorkSecondsErrorLabel") as Label
                    ?? throw new InvalidOperationException("Work seconds error label not found.");
                _restSecondsErrorLabel = FindByName("RestSecondsErrorLabel") as Label
                    ?? throw new InvalidOperationException("Rest seconds error label not found.");
                _setsErrorLabel = FindByName("SetsErrorLabel") as Label
                    ?? throw new InvalidOperationException("Sets error label not found.");

                if (IPlatformApplication.Current == null)
                    throw new InvalidOperationException("Platform application is not initialized.");

            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, "Failed to initialize MainPage.");
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            ClearValidationErrors();

            if (string.IsNullOrWhiteSpace(CollectionName))
            {
                await DisplayAlertAsync("Validation", "Collection name is required.", "OK");
                return;
            }

            int workMinutes = ParseEntryOrDefault(WorkMinutesEntry);
            int workSeconds = ParseEntryOrDefault(WorkSecondsEntry);
            int restMinutes = ParseEntryOrDefault(RestMinutesEntry);
            int restSeconds = ParseEntryOrDefault(RestSecondsEntry);
            int sets = ParseEntryOrDefault(NumberOfSetsEntry);

            bool hasErrors = false;

            if ((workMinutes * 60) + workSeconds < _minSeconds)
            {
                ShowError(_workSecondsErrorLabel, "Work time must be at least 1 second.");
                hasErrors = true;
            }

            if ((restMinutes * 60) + restSeconds < 0)
            {
                ShowError(_restSecondsErrorLabel, "Rest time is invalid.");
                hasErrors = true;
            }

            if (sets < _minSets)
            {
                ShowError(_setsErrorLabel, "Sets must be at least 1.");
                hasErrors = true;
            }

            if (hasErrors)
                return;

            var collection = new WorkoutCollection
            {
                Id = _collectionId,
                Name = CollectionName,
                WorkMinutes = workMinutes,
                WorkSeconds = workSeconds,
                RestMinutes = restMinutes,
                RestSeconds = restSeconds,
                Sets = sets
            };

            try
            {
                await _saveWorkoutCollectionUseCase.ExecuteAsync(collection);
                await DisplayAlertAsync("Saved", "Collection saved successfully.", "OK");
                await _navigationService.GoToAsync("..");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save workout collection.");
                await DisplayAlertAsync("Error", "Could not save collection.", "OK");
            }

        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            ClearValidationErrors();

            if (Shell.Current?.Navigation.NavigationStack.Count > 1)
            {
                await _navigationService.GoToAsync("..");
                return;
            }

            CollectionNameEntry.Text = string.Empty;
            WorkMinutesEntry.Text = string.Empty;
            WorkSecondsEntry.Text = string.Empty;
            RestMinutesEntry.Text = string.Empty;
            RestSecondsEntry.Text = string.Empty;
            NumberOfSetsEntry.Text = string.Empty;
        }

        private static int ParseEntryOrDefault(Entry? entry)
        {
            return int.TryParse(entry?.Text, out int value) ? value : 0;
        }

        private static void ShowError(Label? label, string message)
        {
            if (label is null)
                return;

            label.Text = message;
            label.IsVisible = true;
        }

        private void ClearValidationErrors()
        {
            if (_workSecondsErrorLabel is not null)
            {
                _workSecondsErrorLabel.Text = string.Empty;
                _workSecondsErrorLabel.IsVisible = false;
            }

            if (_restSecondsErrorLabel is not null)
            {
                _restSecondsErrorLabel.Text = string.Empty;
                _restSecondsErrorLabel.IsVisible = false;
            }

            if (_setsErrorLabel is not null)
            {
                _setsErrorLabel.Text = string.Empty;
                _setsErrorLabel.IsVisible = false;
            }
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RequestInitialPermissionsAsync();
            await LoadCollectionAsync();
        }

        private async Task LoadCollectionAsync()
        {
            if (_collectionId == 0 || _isCollectionLoaded)
                return;

            var collection = await _readWorkoutCollectionUseCase.ExecuteAsync(_collectionId);
            if (collection is null)
                return;

            CollectionNameEntry.Text = collection.Name;
            WorkMinutesEntry.Text = collection.WorkMinutes.ToString();
            WorkSecondsEntry.Text = collection.WorkSeconds.ToString();
            RestMinutesEntry.Text = collection.RestMinutes.ToString();
            RestSecondsEntry.Text = collection.RestSeconds.ToString();
            NumberOfSetsEntry.Text = collection.Sets.ToString();
            _isCollectionLoaded = true;
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
    }
}
