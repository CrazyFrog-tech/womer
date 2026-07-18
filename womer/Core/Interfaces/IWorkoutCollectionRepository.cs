using System;
using System.Collections.Generic;
using System.Text;
using womer.Core.Models;

namespace womer.Core.Interfaces
{
    public interface IWorkoutCollectionRepository
    {
        // GetAllAsync
        Task<IEnumerable<WorkoutCollection>> GetAllAsync();
        // GetByIdAsync
        Task<WorkoutCollection?> GetByIdAsync(long id);
        // SaveAsync
        Task<WorkoutCollection> SaveAsync(WorkoutCollection workoutCollection);
        // UpdateAsync
        Task<WorkoutCollection> UpdateAsync(WorkoutCollection workoutCollection);
        // DeleteAsync
        Task DeleteByIdAsync(long id);

    }
}
