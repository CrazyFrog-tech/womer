using womer.Core.Interfaces;

namespace womer.Application.UseCases
{
    public sealed class SetVolumeUseCase
    {
        private readonly IWorkoutSettings _settings;
        private readonly IAudioService _audioService;

        public SetVolumeUseCase(IWorkoutSettings settings, IAudioService audioService)
        {
            _settings = settings;
            _audioService = audioService;
        }

        public void Execute(double newVolume)
        {
            _settings.Volume = newVolume;
            _audioService.SetVolume(newVolume);
        }
    }
}
