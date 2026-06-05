namespace womer.Core.Interfaces
{
    public interface ITimerSoundService
    {
        void PlayTick();
        void PlayStartingWhistle();
        void PlayPhaseSwitch();
        void PlayWorkoutComplete();
    }
}
