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
    }

    private async void OnCopyFacebookClicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync("https://www.facebook.com/share/g/1ZRUbuN9pw/");
    }

}