using System.Collections.ObjectModel;
using womer.Application.UseCases.WorkoutCollectionUseCases;
using womer.Core.Interfaces;
using womer.Core.Models;

namespace womer.Presentation.Pages;

public partial class CollectionsPage : ContentPage
{
	private readonly GetAllWorkoutCollectionsUseCase _getAllWorkoutCollectionsUseCase;
	private readonly ReadWorkoutCollectionUseCase _readWorkoutCollectionUseCase;
	private readonly UpdateWorkoutCollectionOrderUseCase _updateWorkoutCollectionOrderUseCase;
	private readonly DeleteWorkoutCollectionUseCase _deleteWorkoutCollectionUseCase;
	private readonly List<WorkoutCollection> _collections = [];
	private readonly INavigationService _navigationService;

	public ObservableCollection<WorkoutCollection> Collections { get; } = [];

	public CollectionsPage(
		GetAllWorkoutCollectionsUseCase getAllWorkoutCollectionsUseCase,
		ReadWorkoutCollectionUseCase readWorkoutCollectionUseCase,
		UpdateWorkoutCollectionOrderUseCase updateWorkoutCollectionOrderUseCase,
		DeleteWorkoutCollectionUseCase deleteWorkoutCollectionUseCase,
		INavigationService navigationService)
	{
		InitializeComponent();
		_getAllWorkoutCollectionsUseCase = getAllWorkoutCollectionsUseCase ?? throw new ArgumentNullException(nameof(getAllWorkoutCollectionsUseCase));
		_readWorkoutCollectionUseCase = readWorkoutCollectionUseCase ?? throw new ArgumentNullException(nameof(readWorkoutCollectionUseCase));
		_updateWorkoutCollectionOrderUseCase = updateWorkoutCollectionOrderUseCase ?? throw new ArgumentNullException(nameof(updateWorkoutCollectionOrderUseCase));
		_deleteWorkoutCollectionUseCase = deleteWorkoutCollectionUseCase ?? throw new ArgumentNullException(nameof(deleteWorkoutCollectionUseCase));
		_navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
		CollectionsView.ItemsSource = Collections;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await LoadCollectionsAsync();
	}

	private async Task LoadCollectionsAsync()
	{
		var items = await _getAllWorkoutCollectionsUseCase.ExecuteAsync();
		var ordered = items.ToList();

		_collections.Clear();
		_collections.AddRange(ordered);

		Collections.Clear();
		foreach (var collection in ordered)
			Collections.Add(collection);
	}

	private async void MoveCollectionUpButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button { CommandParameter: long collectionId })
			await MoveCollectionAsync(collectionId, -1);
	}

	private async void MoveCollectionDownButton_Clicked(object sender, EventArgs e)
	{
		if (sender is Button { CommandParameter: long collectionId })
			await MoveCollectionAsync(collectionId, 1);
	}

	private async Task MoveCollectionAsync(long collectionId, int offset)
	{
		var currentIndex = _collections.FindIndex(collection => collection.Id == collectionId);
		var newIndex = currentIndex + offset;
		if (currentIndex < 0 || newIndex < 0 || newIndex >= _collections.Count)
			return;

		var collection = _collections[currentIndex];
		_collections.RemoveAt(currentIndex);
		_collections.Insert(newIndex, collection);
		Collections.Move(currentIndex, newIndex);

		await _updateWorkoutCollectionOrderUseCase.ExecuteAsync(_collections);
	}

    private async void PlayCollectionButton_Clicked(object sender, EventArgs e)
    {
		if (sender is not Button { CommandParameter: long collectionId })
            return;

            var collection = await _readWorkoutCollectionUseCase.ExecuteAsync(collectionId);
            if (collection is null)
                return;

            await _navigationService.GoToAsync($"TimerPage?collectionId={collectionId}");
        }

    private async void PlayAllButton_Clicked(object sender, EventArgs e)
	{
		if (_collections.Count == 0)
			return;

		await _navigationService.GoToAsync("TimerPage?playAll=true");
	}

	private async void AddNewButton_Clicked(object sender, EventArgs e)
	{
		await _navigationService.GoToAsync("EditCollectionPage");
	
	}

	private async void EditCollectionButton_Clicked(object sender, EventArgs e)
	{
		if (sender is not Button { CommandParameter: long collectionId })
			return;

		await _navigationService.GoToAsync($"EditCollectionPage?collectionId={collectionId}");
	}

	private async void DeleteCollectionButton_Clicked(object sender, EventArgs e)
	{
		if (sender is not Button { CommandParameter: long collectionId })
			return;

		bool confirm = await DisplayAlert("Delete", "Are you sure you want to delete this collection?", "Yes", "No");
		if (confirm)
		{
			await _deleteWorkoutCollectionUseCase.ExecuteAsync(collectionId);
			await LoadCollectionsAsync();
		}
	}
}
