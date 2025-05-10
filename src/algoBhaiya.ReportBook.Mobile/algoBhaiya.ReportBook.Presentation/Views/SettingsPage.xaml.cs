using algoBhaiya.ReportBook.Presentation.ViewModels;

namespace algoBhaiya.ReportBook.Presentation.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    private async void OnModificationDurationInfoTapped(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Lock Daily Form",
            "ডেইলি ফরমটি কতদিন পর্যন্ত এডিট করতে পারবেন, এরপর ফরমটি locked হয়ে যাবে (ভিউ করতে পারবেন, তবে এডিট করতে পারবেন না), এটি আপনি configure করে দিতে পারবেন।",
            "ঠিক আছে");
    }

    private async void OnDataRemovalPeriodInfoTapped(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Data Removal Period",
            "অ্যাপের সকল ডাটা আপনার ডিভাইসের internal storage এ stored হচ্ছে। আপনি last কত মাসের ডাটা আপনার ডিভাইসে রাখতে চাচ্ছেন, তা আপনি configure করে দিতে পারবেন। এর পূর্বের মাসের ডাটাগুলো ক্রমান্বয়ে ডিলিট হতে থাকবে।",
            "ঠিক আছে");
    }
}