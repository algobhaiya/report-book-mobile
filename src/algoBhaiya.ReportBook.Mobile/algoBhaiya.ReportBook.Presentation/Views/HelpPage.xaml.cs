namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class HelpPage : ContentPage
{
	public HelpPage()
	{
		InitializeComponent();
	}

    private async void OnCopyEmailClicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync("algobhaiya@gmail.com");
        await DisplayAlert("Copied", "Email address copied to clipboard.", "OK");
    }

    private async void OnCopyFacebookClicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync("https://www.facebook.com/share/g/1ZRUbuN9pw/");
        await DisplayAlert("Copied", "Facebook group link copied to clipboard.", "OK");
    }

}