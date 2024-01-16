using Camera.MAUI;
using CommunityToolkit.Maui;
using Plugin.Maui.Audio;

namespace PathWays
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                  .UseMauiCommunityToolkit()
                 .UseMauiCameraView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddTransient<MainPage>();


            return builder.Build();
        }
    }
}
