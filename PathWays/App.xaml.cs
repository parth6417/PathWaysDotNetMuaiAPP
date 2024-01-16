using Plugin.Maui.Audio;

namespace PathWays
{
    public partial class App : Application
    {
        public App(IAudioManager audioManager)
        {
            InitializeComponent();

            MainPage = new Home(audioManager);
        }
    }
}
