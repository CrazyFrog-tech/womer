using womer.Core.Interfaces;
using womer.Core.Models;

namespace womer.Application.UseCases.WorkoutCollectionUseCases
{
    public class GetAllWorkoutCollectionsUseCase
    {
        private readonly IWorkoutCollectionRepository _repository;

        public GetAllWorkoutCollectionsUseCase(IWorkoutCollectionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<WorkoutCollection>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
