namespace Mp3MultiCutter;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private async void UploadButton_OnClicked(object sender, EventArgs e)
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
        }
        catch (Exception ex)
        {
            await this.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void MainPage_OnUnloaded(object sender, EventArgs e)
    {
        this.mediaElement.Handler?.DisconnectHandler();
    }
}
