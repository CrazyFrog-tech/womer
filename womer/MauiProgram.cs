using Microsoft.Extensions.Logging;
using womer.Application.UseCases;
using womer.Core.Interfaces;
using womer.Infrastructure.Audio;
using womer.Infrastructure.Device;
using womer.Infrastructure.Notifications;
using womer.Infrastructure.Services;

namespace womer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<IWorkoutSettings, WorkoutService>();
            builder.Services.AddSingleton<IAudioService, AudioService>();
            builder.Services.AddSingleton<ITimerNotificationService, TimerNotificationService>();
            builder.Services.AddSingleton<ITimerSoundService, TimerSoundService>();
            builder.Services.AddTransient<GetInitialVolumeUseCase>();
            builder.Services.AddTransient<SetVolumeUseCase>();
            builder.Services.AddTransient<LoadWorkoutPlanUseCase>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<TimerPage>();



#if DEBUG
            builder.Logging.ClearProviders();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
            builder.Logging.AddFilter("womer", LogLevel.Debug);
#endif

            return builder.Build();
        }
    }
}
