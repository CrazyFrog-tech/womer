using System.Collections.Generic;
using womer.Core.Interfaces;
using womer.Core.Models;

namespace womer.Application.UseCases.WorkoutCollectionUseCases
{
    public class UpdateWorkoutCollectionOrderUseCase
    {
        private readonly IWorkoutCollectionRepository _repository;

        public UpdateWorkoutCollectionOrderUseCase(IWorkoutCollectionRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(IEnumerable<WorkoutCollection> collections)
        {
            await _repository.UpdateOrderAsync(collections);
        }
    }
}
