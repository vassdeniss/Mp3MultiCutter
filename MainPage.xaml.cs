using CommunityToolkit.Maui.Core.Primitives;

using System.Collections.ObjectModel;

namespace Mp3MultiCutter;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    public ObservableCollection<TimeSpan> Cuts { get; } = new();

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
                throw new Exception("File is not mmp3!");
            }

            this.mediaElement.Source = result.FullPath;
        }
        catch (Exception ex)
        {
            await this.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void PlayButton_OnClicked(object? sender, EventArgs e)
    {
        if (this.mediaElement.CurrentState is MediaElementState.Stopped or MediaElementState.Paused)
        {
            this.mediaElement.Play();
        }
        else if (this.mediaElement.CurrentState == MediaElementState.Playing)
        {
            this.mediaElement.Pause();
        }
    }

    private void MediaElement_OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            this.progressLabel.Text = $@"{e.Position:mm\:ss} / {this.mediaElement.Duration:mm\:ss}";
        });
    }

    private async void Rewind60Button_OnClicked(object? sender, EventArgs e)
    {
        await this.mediaElement.SeekTo(this.mediaElement.Position.Subtract(TimeSpan.FromSeconds(60)));
    }

    private async void Rewind15Button_OnClicked(object? sender, EventArgs e)
    {
        await this.mediaElement.SeekTo(this.mediaElement.Position.Subtract(TimeSpan.FromSeconds(15)));
    }

    private async void Forward15Button_OnClicked(object? sender, EventArgs e)
    {
        await this.mediaElement.SeekTo(this.mediaElement.Position.Add(TimeSpan.FromSeconds(15)));
    }

    private async void Forward60Button_OnClicked(object? sender, EventArgs e)
    {
        await this.mediaElement.SeekTo(this.mediaElement.Position.Add(TimeSpan.FromSeconds(60)));
    }

    private void MainPage_OnUnloaded(object? sender, EventArgs e)
    {
        this.mediaElement.Handler?.DisconnectHandler();
    }

    private void CutButton_OnClicked(object? sender, EventArgs e)
    {
        this.Cuts.Add(this.mediaElement.Position);
    }
}
