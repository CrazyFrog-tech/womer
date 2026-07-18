using System;
using System.Collections.Generic;
using System.Text;
using womer.Core.Interfaces;
using womer.Core.Models;

namespace womer.Application.UseCases.WorkoutCollectionUseCases
{
    public class ReadWorkoutCollectionUseCase
    {
        private readonly IWorkoutCollectionRepository _repository;

        public ReadWorkoutCollectionUseCase(IWorkoutCollectionRepository repository)
        {
            _repository = repository;
        }

        public async Task<WorkoutCollection?> ExecuteAsync(long id) => await _repository.GetByIdAsync(id);
    }
}
