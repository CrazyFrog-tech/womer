using System;
using System.Collections.Generic;
using System.Text;

namespace womer.Core.Interfaces
{
    public interface IWorkoutSettings
    {
        double Volume { get; set; }
        int WorkMinutes { get; set; }
        int WorkSeconds { get; set; }
        int RestMinutes { get; set; }
        int RestSeconds { get; set; }
        int Sets { get; set; }
    }
}
