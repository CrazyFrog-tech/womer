using womer.Core.Interfaces;

#if ANDROID
using Android.Content;
using Android.Media;
#endif

namespace womer.Infrastructure.Device
{
    public sealed class AudioService : IAudioService
    {
        public double GetVolume()
        {
#if ANDROID
            var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
            if (audioManager is null)
                return 0;

            int max = audioManager.GetStreamMaxVolume(Android.Media.Stream.Music);
            int current = audioManager.GetStreamVolume(Android.Media.Stream.Music);

            if (max <= 0)
                return 0;

            return (double)current / max;
#else
            return 0;
#endif
        }

        public void SetVolume(double volume)
        {
#if ANDROID
            var audioManager = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
            if (audioManager is null)
                return;

            int max = audioManager.GetStreamMaxVolume(Android.Media.Stream.Music);
            if (max <= 0)
                return;

            int target = (int)Math.Round(Math.Clamp(volume, 0, 1) * max);
            audioManager.SetStreamVolume(Android.Media.Stream.Music, target, 0);
#endif
        }
    }
}
