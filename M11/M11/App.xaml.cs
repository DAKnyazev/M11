using System;
using System.Threading.Tasks;
using M11.Common.Models;
using M11.Services;
using Plugin.SecureStorage;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Skip)]
namespace M11
{
	public partial class App : Application
	{
	    private const string LoginKeyName = "Login";
	    private const string PasswordKeyName = "Password";
	    public static int CachingTimeInMinutes { get; set; }
        public static Credentials Credentials { get; set; }
        public static Info Info { get; set; }
        private static bool IsMainPageVisible { get; set; }
	    private static bool IsStatisticPageVisible { get; set; }
	    private static bool IsPaymentPageVisible { get; set; }
	    private static bool IsSettingsPageVisible { get; set; }
        
        static App()
        {
            CachingTimeInMinutes = 3;
            Credentials = new Credentials();
	        Info = new Info();
	        IsMainPageVisible = true;
	        IsStatisticPageVisible = false;
	        IsPaymentPageVisible = false;
	        IsSettingsPageVisible = false;
        }

        public App()
		{
			InitializeComponent();
            try
		    {
		        Credentials.Login = CrossSecureStorage.Current.HasKey(LoginKeyName) ? CrossSecureStorage.Current.GetValue(LoginKeyName) : string.Empty;
		        Credentials.Password = CrossSecureStorage.Current.HasKey(PasswordKeyName) ? CrossSecureStorage.Current.GetValue(PasswordKeyName) : string.Empty;
            }
		    catch
		    {
		        Credentials.Login = string.Empty;
		        Credentials.Password = string.Empty;
		    }
		    
		    if (!string.IsNullOrWhiteSpace(Credentials.Login) 
		        && !string.IsNullOrWhiteSpace(Credentials.Password))
		    {
		        MainPage = new NavigationPage(new MainPage());
            }
		    else
		    {
		        MainPage = new NavigationPage(new AuthPage());
            }
		}

		protected override void OnStart()
		{
		    // Handle when your app starts
        }

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}

	    public static void Exit()
	    {
	        CrossSecureStorage.Current.DeleteKey(LoginKeyName);
	        CrossSecureStorage.Current.DeleteKey(PasswordKeyName);
            Info = new Info();
            Credentials = new Credentials();
        }

	    public static async Task<bool> TryGetInfo()
	    {
	        return await TryGetInfo(Credentials.Login, Credentials.Password);
	    }

	    public static async Task<bool> TryGetInfo(string login, string password)
	    {
	        if (Info.RequestDate > DateTime.Now.AddMinutes(-CachingTimeInMinutes))
	        {
	            return true;
	        }
	        var info = await new InfoService().GetInfo(login, password);
	        if (!string.IsNullOrWhiteSpace(info.ContractNumber))
	        {
	            Info = info;
	            CrossSecureStorage.Current.SetValue(LoginKeyName, login);
	            CrossSecureStorage.Current.SetValue("Password", password);
                Credentials.Login = login;
	            Credentials.Password = password;

	            return true;
	        }

	        return false;
	    }

	    private void MainButton_OnClicked(object sender, EventArgs e)
	    {
	        ClearMenu();
	        IsMainPageVisible = true;
	        MainPage = new NavigationPage(new MainPage());
        }

	    private void StatisticButton_OnClicked(object sender, EventArgs e)
	    {
	        ClearMenu();
	        IsStatisticPageVisible = true;
	        MainPage = new NavigationPage(new StatisticPage());
	    }

	    private void PaymentButton_OnClicked(object sender, EventArgs e)
	    {
	        ClearMenu();
	        IsPaymentPageVisible = true;
	        MainPage = new NavigationPage(new PaymentPage());
	    }

	    private void SettingsButton_OnClicked(object sender, EventArgs e)
	    {
	        ClearMenu();
	        IsSettingsPageVisible = true;
	        MainPage = new NavigationPage(new SettingsPage());
        }

	    private static void ClearMenu()
	    {
	        IsMainPageVisible = false;
	        IsStatisticPageVisible = false;
	        IsPaymentPageVisible = false;
	        IsSettingsPageVisible = false;
	    }
	}
}
