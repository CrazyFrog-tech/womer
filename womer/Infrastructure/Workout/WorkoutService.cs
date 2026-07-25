using womer.Core.Interfaces;
using womer.Infrastructure.Settings;

namespace womer.Infrastructure.Services
{
    public sealed class WorkoutService : IWorkoutSettings
    {
        private readonly IPreferences _preferences;

        public WorkoutService(IPreferences preferences)
        {
            if (preferences == null)    throw new ArgumentNullException(nameof(preferences));
            _preferences = preferences;
        }

        public double Volume
        {
            get => _preferences.Get(StorageKeys.WorkoutSettings.Volume, 1.0);
            set => _preferences.Set(StorageKeys.WorkoutSettings.Volume, value);
        }

        public int WorkMinutes
        {
            get => _preferences.Get(StorageKeys.WorkoutSettings.WorkMinutes, 0);
            set => _preferences.Set(StorageKeys.WorkoutSettings.WorkMinutes, value);
        }

        public int WorkSeconds
        {
            get => _preferences.Get(StorageKeys.WorkoutSettings.WorkSeconds, 10);
            set => _preferences.Set(StorageKeys.WorkoutSettings.WorkSeconds, value);
        }

        public int RestMinutes
        {
            get => _preferences.Get(StorageKeys.WorkoutSettings.RestMinutes, 0);
            set => _preferences.Set(StorageKeys.WorkoutSettings.RestMinutes, value);
        }

        public int RestSeconds
        {
            get => _preferences.Get(StorageKeys.WorkoutSettings.RestSeconds, 5);
            set => _preferences.Set(StorageKeys.WorkoutSettings.RestSeconds, value);
        }

        public int Sets
        {
            get => _preferences.Get(StorageKeys.WorkoutSettings.Sets, 1);
            set => _preferences.Set(StorageKeys.WorkoutSettings.Sets, value);
        }
    }
}
