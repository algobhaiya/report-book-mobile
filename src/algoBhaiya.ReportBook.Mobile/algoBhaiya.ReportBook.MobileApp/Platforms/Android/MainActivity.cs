using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using AndroidX.Activity;
using AndroidX.Core.View;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

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
        OnBackPressedDispatcher.AddCallback(this, new BackNavigationCallback());
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

    private sealed class BackNavigationCallback : OnBackPressedCallback
    {
        public BackNavigationCallback() : base(true)
        {
        }

        public override void HandleOnBackPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var shellNavigation = Shell.Current?.Navigation;
                if (shellNavigation is not null)
                {
                    if (shellNavigation.ModalStack.Count > 0)
                    {
                        await shellNavigation.PopModalAsync();
                        return;
                    }

                    if (shellNavigation.NavigationStack.Count > 1)
                    {
                        await shellNavigation.PopAsync();
                        return;
                    }
                }

                var mainNavigation = global::Microsoft.Maui.Controls.Application.Current?.MainPage?.Navigation;
                if (mainNavigation is null)
                {
                    global::Microsoft.Maui.Controls.Application.Current?.Quit();
                    return;
                }

                if (mainNavigation.ModalStack.Count > 0)
                {
                    await mainNavigation.PopModalAsync();
                    return;
                }

                if (mainNavigation.NavigationStack.Count > 1)
                {
                    await mainNavigation.PopAsync();
                    return;
                }

                global::Microsoft.Maui.Controls.Application.Current?.Quit();
            });
        }
    }
}
