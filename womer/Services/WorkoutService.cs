using System;
using System.Collections.Generic;
using System.Text;

namespace womer.Services
{
    public interface IWorkoutService
    {
        double Volume { get; set; }
        int WorkMinutes { get; set; }
        int WorkSeconds { get; set; }
        int RestMinutes { get; set; }
        int RestSeconds { get; set; }
        int Sets { get; set; }
    }

    public sealed class WorkoutService : IWorkoutService
    {
        private const string VolumeKey = "Volume";
        private const string WorkMinutesKey = "WorkMinutes";
        private const string WorkSecondsKey = "WorkSeconds";
        private const string RestMinutesKey = "RestMinutes";
        private const string RestSecondsKey = "RestSeconds";
        private const string SetsKey = "Sets";

        private static IPreferences Preferences => Microsoft.Maui.Storage.Preferences.Default;

        public double Volume
        {
            get => Preferences.Get(VolumeKey, 1.0);
            set => Preferences.Set(VolumeKey, value);
        }

        public int WorkMinutes
        {
            get => Preferences.Get(WorkMinutesKey, 0);
            set => Preferences.Set(WorkMinutesKey, value);
        }

        public int WorkSeconds
        {
            get => Preferences.Get(WorkSecondsKey, 10);
            set => Preferences.Set(WorkSecondsKey, value);
        }

        public int RestMinutes
        {
            get => Preferences.Get(RestMinutesKey, 0);
            set => Preferences.Set(RestMinutesKey, value);
        }

        public int RestSeconds
        {
            get => Preferences.Get(RestSecondsKey, 5);
            set => Preferences.Set(RestSecondsKey, value);
        }

        public int Sets
        {
            get => Preferences.Get(SetsKey, 1);
            set => Preferences.Set(SetsKey, value);
        }
    }
}
