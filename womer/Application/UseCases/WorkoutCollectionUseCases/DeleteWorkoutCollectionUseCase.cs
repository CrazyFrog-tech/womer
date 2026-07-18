using System;
using System.Collections.Generic;
using System.Text;
using womer.Core.Interfaces;

namespace womer.Application.UseCases.WorkoutCollectionUseCases
{
    public class DeleteWorkoutCollectionUseCase
    {
        private readonly IWorkoutCollectionRepository _repository;

        public DeleteWorkoutCollectionUseCase(IWorkoutCollectionRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(long id)
        {
            await _repository.DeleteByIdAsync(id);
        }
    }
}
