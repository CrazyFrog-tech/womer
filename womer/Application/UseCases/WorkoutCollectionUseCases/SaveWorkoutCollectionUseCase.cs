using System;
using System.Collections.Generic;
using System.Text;
using womer.Core.Interfaces;
using womer.Core.Models;

namespace womer.Application.UseCases.WorkoutCollectionUseCases
{
    public class SaveWorkoutCollectionUseCase
    {
        private readonly IWorkoutCollectionRepository _repository;

        public SaveWorkoutCollectionUseCase(IWorkoutCollectionRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(WorkoutCollection collection)
        {
            if (collection.Id == 0)
                await _repository.SaveAsync(collection);
            else
                await _repository.UpdateAsync(collection);
        }
    }
}
