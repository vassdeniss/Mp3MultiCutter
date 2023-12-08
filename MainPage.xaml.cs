namespace Mp3MultiCutter;

public partial class MainPage : ContentPage
{
    private const int UPDATE_INTERVAL_MILLISECONDS = 1000;

    private bool _isPlaying;
    private TimeSpan _totalDuration = TimeSpan.Zero;
    private TimeSpan _currentPosition = TimeSpan.Zero;
    private IDispatcherTimer? _timer;

    public MainPage()
    {
        this.InitializeComponent();
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

            this.playButton.IsEnabled = true;
            this.rewind15Button.IsEnabled = true;
            this.rewind60Button.IsEnabled = true;
            this.forward15Button.IsEnabled = true;
            this.forward60Button.IsEnabled = true;

            this.mediaElement.Source = result.FullPath;
            this.mediaElement.MediaOpened += MediaElementOnMediaOpened;
            this.playButton.ImageSource = "pause_16.png";
            this._isPlaying = true;

            this._timer = this.SetUpAudioTimer();
        }
        catch (Exception ex)
        {
            await this.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void MediaElementOnMediaOpened(object? sender, EventArgs e)
    {
        IDispatcherTimer? durationTimer = Application.Current?.Dispatcher.CreateTimer();
        if (durationTimer is null)
        {
            return;
        }

        durationTimer.Interval = TimeSpan.FromSeconds(1);
        durationTimer.Tick += (_, _) =>
        {
            if (this.mediaElement.Duration == TimeSpan.Zero)
            {
                return;
            }

            this._totalDuration = this.mediaElement.Duration;
            durationTimer.Stop();
        };

        durationTimer.Start();
    }

    private void PlayButton_OnClicked(object? sender, EventArgs e)
    {
        if (this._isPlaying)
        {
            this.playButton.ImageSource = "play_16.png";
            this._timer!.Stop();
            this.mediaElement.Pause();
            this._isPlaying = false;
        }
        else
        {
            this.playButton.ImageSource = "pause_16.png";
            this._timer!.Start();
            this.mediaElement.Play();
            this._isPlaying = true;
        }
    }

    private IDispatcherTimer? SetUpAudioTimer()
    {
        IDispatcherTimer? timer = Application.Current?.Dispatcher.CreateTimer();
        if (timer is null)
        {
            return null;
        }

        timer.Interval = TimeSpan.FromMilliseconds(UPDATE_INTERVAL_MILLISECONDS);

        timer.Tick += (_, _) =>
        {
            this._currentPosition = this.mediaElement.Position;
            this.UpdateTimeLabel();
        };

        timer.Start();

        return timer;
    }

    private void Rewind60Button_OnClicked(object? sender, EventArgs e)
    {
        this._currentPosition = this._currentPosition.Subtract(TimeSpan.FromSeconds(60));
        this.mediaElement.SeekTo(this._currentPosition);
        this.UpdateTimeLabel();
    }

    private void Rewind15Button_OnClicked(object? sender, EventArgs e)
    {
        this._currentPosition = this._currentPosition.Subtract(TimeSpan.FromSeconds(15));
        this.mediaElement.SeekTo(this._currentPosition);
        this.UpdateTimeLabel();
    }

    private void Forward15Button_OnClicked(object? sender, EventArgs e)
    {
        this._currentPosition = this._currentPosition.Add(TimeSpan.FromSeconds(15));
        this.mediaElement.SeekTo(this._currentPosition);
        this.UpdateTimeLabel();
    }

    private void Forward60Button_OnClicked(object? sender, EventArgs e)
    {
        this._currentPosition = this._currentPosition.Add(TimeSpan.FromSeconds(60));
        this.mediaElement.SeekTo(this._currentPosition);
        this.UpdateTimeLabel();
    }

    private void UpdateTimeLabel()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            this.progressLabel.Text = $@"{this._currentPosition:mm\:ss} / {this._totalDuration:mm\:ss}";
        });
    }

    private void MainPage_OnUnloaded(object? sender, EventArgs e)
    {
        this.mediaElement.Handler?.DisconnectHandler();
    }
}
