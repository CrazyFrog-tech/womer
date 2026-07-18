using System;
using System.Collections.Generic;
using System.Text;

namespace womer.Core.Models
{
    public sealed class WorkoutCollection
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int WorkMinutes { get; set; }
        public int WorkSeconds { get; set; }
        public int RestMinutes { get; set; }
        public int RestSeconds { get; set; }
        public int Sets { get; set; }

    }
}
