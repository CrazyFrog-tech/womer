using System.Collections.ObjectModel;
using womer.Application.UseCases.WorkoutCollectionUseCases;
using womer.Core.Models;

namespace womer.Presentation.Pages;

public partial class CollectionsPage : ContentPage
{
	private readonly GetAllWorkoutCollectionsUseCase _getAllWorkoutCollectionsUseCase;
	private readonly ReadWorkoutCollectionUseCase _readWorkoutCollectionUseCase;
	private readonly List<WorkoutCollection> _collections = [];

	public ObservableCollection<WorkoutCollection> Collections { get; } = [];

	public CollectionsPage(
		GetAllWorkoutCollectionsUseCase getAllWorkoutCollectionsUseCase,
		ReadWorkoutCollectionUseCase readWorkoutCollectionUseCase)
	{
		InitializeComponent();
		_getAllWorkoutCollectionsUseCase = getAllWorkoutCollectionsUseCase ?? throw new ArgumentNullException(nameof(getAllWorkoutCollectionsUseCase));
		_readWorkoutCollectionUseCase = readWorkoutCollectionUseCase ?? throw new ArgumentNullException(nameof(readWorkoutCollectionUseCase));
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
		var ordered = items
			.OrderBy(collection => collection.Name, StringComparer.OrdinalIgnoreCase)
			.ToList();

		_collections.Clear();
		_collections.AddRange(ordered);

		Collections.Clear();
		foreach (var collection in ordered)
			Collections.Add(collection);
	}

	private async void PlayCollectionButton_Clicked(object sender, EventArgs e)
	{
		if (sender is not Button { CommandParameter: long collectionId })
			return;

		var collection = await _readWorkoutCollectionUseCase.ExecuteAsync(collectionId);
		if (collection is null)
			return;

		await Shell.Current.GoToAsync($"TimerPage?collectionId={collectionId}");
	}

	private async void PlayAllButton_Clicked(object sender, EventArgs e)
	{
		if (_collections.Count == 0)
			return;

		await Shell.Current.GoToAsync($"TimerPage?collectionId={_collections[0].Id}");
	}

	private async void AddNewButton_Clicked(object sender, EventArgs e)
	{
		await DisplayAlertAsync("Collections", "Add New will be implemented next.", "OK");
	}

	private async void EditCollectionButton_Clicked(object sender, EventArgs e)
	{
		if (sender is not Button { CommandParameter: long })
			return;

		await DisplayAlertAsync("Collections", "Edit will be implemented next.", "OK");
	}
}
