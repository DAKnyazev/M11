using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.V7.App;

namespace M11.Droid
{
    [Activity(
        Label = "m11", 
        Icon = "@mipmap/icon", 
        Theme = "@style/MainTheme.Splash", 
        MainLauncher = true, 
        NoHistory = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, 
        ScreenOrientation = ScreenOrientation.Portrait,
        Exported = true)]
    public class SplashActivity : AppCompatActivity
    {
        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            Startup();
        }

        // Simulates background work that happens behind the splash screen
        private void Startup()
        {
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}