using womer.Core.Interfaces;

#if ANDROID
using Android.Media;
#endif

namespace womer.Infrastructure.Audio
{
    public sealed class TimerSoundService : ITimerSoundService, IDisposable
    {
#if ANDROID
        private readonly ToneGenerator _toneGenerator = new(Android.Media.Stream.Music, 100);
#endif

        public void PlayTick()
        {
#if ANDROID
            _toneGenerator.StartTone(Tone.SupBusy, 250);
#endif
        }

        public void PlayStartingWhistle()
        {
#if ANDROID
            _toneGenerator.StartTone(Tone.CdmaAbbrReorder, 500);
#endif
        }

        public void PlayPhaseSwitch()
        {
#if ANDROID
            _toneGenerator.StartTone(Tone.CdmaAbbrReorder, 700);
#endif
        }

        public void PlayWorkoutComplete()
        {
#if ANDROID
            _toneGenerator.StartTone(Tone.CdmaAlertCallGuard, 1500);
#endif
        }

        public void Dispose()
        {
#if ANDROID
            _toneGenerator.Release();
            _toneGenerator.Dispose();
#endif
        }
    }
}
