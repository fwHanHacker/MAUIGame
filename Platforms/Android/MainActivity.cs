using Android.Views;
using Android.OS;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Android.App;
using Android.Content.PM;
using Microsoft.Maui.Controls;

namespace Games
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 设置全屏显示，隐藏状态栏和导航栏（可根据需求调整）
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            // 以下代码还可以进一步优化沉浸式全屏体验（可选）
            // if (Build.VERSION.SdkInt >= Build.VERSION_CODES.R)
            // {
            //     Window.SetDecorFitsSystemWindows(false);
            //     Window.InsetsController?.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
            // }
            // else
            // {
            //     Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // }
        }
    }
}