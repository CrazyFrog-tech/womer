namespace womer.Core.Models
{
    public sealed class WorkoutPlan
    {
        public int TotalWorkSeconds { get; init; }
        public int TotalRestSeconds { get; init; }
        public int TotalSets { get; init; }

        public bool IsValid => TotalWorkSeconds > 0;
    }
}
