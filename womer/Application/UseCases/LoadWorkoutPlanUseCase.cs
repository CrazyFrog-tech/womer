using womer.Core.Interfaces;
using womer.Core.Models;

namespace womer.Application.UseCases
{
    public sealed class LoadWorkoutPlanUseCase
    {
        private readonly IWorkoutSettings _settings;

        public LoadWorkoutPlanUseCase(IWorkoutSettings settings)
        {
            _settings = settings;
        }

        public WorkoutPlan Execute()
        {
            return new WorkoutPlan
            {
                TotalWorkSeconds = (_settings.WorkMinutes * 60) + _settings.WorkSeconds,
                TotalRestSeconds = (_settings.RestMinutes * 60) + _settings.RestSeconds,
                TotalSets = Math.Max(1, _settings.Sets)
            };
        }
    }
}
