using womer.Core.Interfaces;

namespace womer.Application.UseCases
{
    public sealed class GetInitialVolumeUseCase
    {
        private readonly IWorkoutSettings _settings;
        private readonly IAudioService _audioService;

        public GetInitialVolumeUseCase(IWorkoutSettings settings, IAudioService audioService)
        {
            _settings = settings;
            _audioService = audioService;
        }

        public double Execute()
        {
            double volume = _audioService.GetVolume();
            if (volume <= 0)
                volume = _settings.Volume;

            return volume;
        }
    }
}
