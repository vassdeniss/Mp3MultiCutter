using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;

using NAudio.Lame;
using NAudio.Wave;

using System.Collections.ObjectModel;

namespace Mp3MultiCutter;

public partial class MainPage : ContentPage
{
    private string? _inputFilePath = null;

    public MainPage()
    {
        this.InitializeComponent();

        this.BindingContext = this;
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

            this._inputFilePath = result.FullPath;
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
        this.removeButton.IsEnabled = true;
    }

    private void RemoveButton_OnClicked(object? sender, EventArgs e)
    {
        this.Cuts.Remove(this.Cuts.Last());
        this.removeButton.IsEnabled = this.Cuts.Count > 0;
    }

    private async void SaveButton_OnClicked(object? sender, EventArgs e)
    {
        if (this.Cuts.Count < 2 || this._inputFilePath is null)
        {
            return;
        }

        FolderPickerResult folder = await FolderPicker.Default.PickAsync();
        if (folder.Folder is null)
        {
            return;
        }

        for (int i = 0; i < this.Cuts.Count - 1; i++)
        {
            TimeSpan startTime = this.Cuts[i];
            TimeSpan endTime = this.Cuts[i + 1];

            this.SaveSegment(folder.Folder.Path + $@"\{i + 1:D2}.mp3", startTime, endTime);
        }

        await this.DisplayAlert("Success", "Files saved!", "OK");
    }

    public void SaveSegment(string outputFilePath, TimeSpan startTime, TimeSpan endTime)
    {
        using Mp3FileReader reader = new(this._inputFilePath);
        reader.CurrentTime = startTime;

        using LameMP3FileWriter writer = new(outputFilePath, reader.WaveFormat, LAMEPreset.STANDARD);

        byte[] buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
        while (reader.CurrentTime < endTime)
        {
            int bytesRead = reader.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }


            writer.Write(buffer, 0, bytesRead);
        }
    }
}
