using Plugin.Maui.Audio;

namespace PathWays;

public partial class Home : ContentPage
{
    private readonly IAudioManager audioManager;

    public Home(IAudioManager audioManager)
    {
        this.audioManager = audioManager;
        InitializeComponent();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new MainPage(this.audioManager);
    }
}