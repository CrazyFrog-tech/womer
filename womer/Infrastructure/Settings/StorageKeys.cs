using System;
using System.Collections.Generic;
using System.Text;

namespace womer.Infrastructure.Settings
{
    internal static class StorageKeys
    {
        public static class WorkoutSettings
        {
            public const string Volume = "Volume";
            public const string WorkMinutes = "WorkMinutes";
            public const string WorkSeconds = "WorkSeconds";
            public const string RestMinutes = "RestMinutes";
            public const string RestSeconds = "RestSeconds";
            public const string Sets = "Sets";
        }

        public static class WorkoutCollection
        {
            public const string List = "WorkoutCollections";
        }
    }
}
