using Games.SQL;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Games
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

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            string dbPath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "database.db3");
            builder.Services.AddSingleton<SettingRepository>(s => ActivatorUtilities.CreateInstance<SettingRepository>(s, dbPath));
            return builder.Build();
        }
    }
}
