using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using AndroidX.Core.View;

namespace algoBhaiya.ReportBook.MobileApp;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        ApplySystemBarTheme();
    }

    protected override void OnResume()
    {
        base.OnResume();

        ApplySystemBarTheme();
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);

        ApplySystemBarTheme();
    }

    private void ApplySystemBarTheme()
    {
        if (Window?.DecorView is null)
        {
            return;
        }

        var isDarkTheme =
            (Resources?.Configuration?.UiMode & UiMode.NightMask)
            == UiMode.NightYes;

        var controller = WindowCompat.GetInsetsController(
            Window,
            Window.DecorView);

        if (controller is null)
        {
            return;
        }

        // Light theme:
        //   Dark status-bar icons
        //
        // Dark theme:
        //   Light status-bar icons
        controller.AppearanceLightStatusBars = !isDarkTheme;

        // Also keep navigation-bar icons appropriate for the theme.
        controller.AppearanceLightNavigationBars = !isDarkTheme;
    }
}