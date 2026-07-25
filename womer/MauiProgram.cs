using Microsoft.Extensions.Logging;
using womer.Application.UseCases;
using womer.Application.UseCases.WorkoutCollectionUseCases;
using womer.Core.Interfaces;
using womer.Infrastructure.Audio;
using womer.Infrastructure.Device;
using womer.Infrastructure.Navigation;
using womer.Infrastructure.Notifications;
using womer.Infrastructure.Repository;
using womer.Infrastructure.Services;
using womer.Presentation.Pages;

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

            // Infrastructure
            builder.Services.AddSingleton<IPreferences>(_ => Microsoft.Maui.Storage.Preferences.Default);
            builder.Services.AddSingleton<IWorkoutSettings, WorkoutService>();
            builder.Services.AddSingleton<IAudioService, AudioService>();
            builder.Services.AddSingleton<ITimerNotificationService, TimerNotificationService>();
            builder.Services.AddSingleton<ITimerSoundService, TimerSoundService>();
            builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
            builder.Services.AddSingleton<IWorkoutCollectionRepository, WorkoutCollectionRepository>();

            // Use cases
            builder.Services.AddTransient<GetInitialVolumeUseCase>();
            builder.Services.AddTransient<SetVolumeUseCase>();
            builder.Services.AddTransient<LoadWorkoutPlanUseCase>();
            builder.Services.AddTransient<GetAllWorkoutCollectionsUseCase>();
            builder.Services.AddTransient<ReadWorkoutCollectionUseCase>();
            builder.Services.AddTransient<SaveWorkoutCollectionUseCase>();
            builder.Services.AddTransient<UpdateWorkoutCollectionOrderUseCase>();
            builder.Services.AddTransient<DeleteWorkoutCollectionUseCase>();
            builder.Services.AddTransient<LoadWorkoutPlanFromWorkoutCollectionUseCase>();

            // Pages
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<EditCollectionPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<TimerPage>();
            builder.Services.AddTransient<CollectionsPage>();

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