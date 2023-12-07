namespace Mp3MultiCutter;

public partial class MainPage : ContentPage
{
    private const int UPDATE_INTERVAL_MILLISECONDS = 100;

    private bool _isPlaying = false;
    private TimeSpan _totalDuration = TimeSpan.Zero;
    private TimeSpan _currentPosition = TimeSpan.Zero;

    public MainPage()
    {
        this.InitializeComponent();
        this.mediaElement.WidthRequest = DeviceDisplay
            .Current
            .MainDisplayInfo
            .Width;
    }

    private async void UploadButton_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            FileResult? result = await FilePicker.PickAsync();
            if (result is null)
            {
                return;
            }

            if (!result.FileName.EndsWith("mp3", StringComparison.OrdinalIgnoreCase))
            {
                await this.DisplayAlert("Error", "File is not mmp3!", "OK");
                return;
            }

            this.mediaElement.Source = result.FullPath;
            this._totalDuration = this.mediaElement.Duration;
            this.mediaElement.Play();
        }
        catch (Exception ex)
        {
            await this.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void PlayButton_OnClicked(object? sender, EventArgs e)
    {
        this.mediaElement.Play();
        this._isPlaying = true;
    }

    private void MainPage_OnUnloaded(object? sender, EventArgs e)
    {
        this.mediaElement.Handler?.DisconnectHandler();
    }
}
