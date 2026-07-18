using System;
using System.Collections.Generic;
using System.Text;
using womer.Core.Interfaces;
using womer.Core.Models;

namespace womer.Application.UseCases.WorkoutCollectionUseCases
{
    public class LoadWorkoutPlanFromWorkoutCollectionUseCase
    {
        public WorkoutPlan Execute(WorkoutCollection collection)
        {
            return new WorkoutPlan
            {
                TotalWorkSeconds = (collection.WorkMinutes * 60) + collection.WorkSeconds,
                TotalRestSeconds = (collection.RestMinutes * 60) + collection.RestSeconds,
                TotalSets = Math.Max(1, collection.Sets)
            };
        }
    }
}
