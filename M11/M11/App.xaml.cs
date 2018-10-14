using System;
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
        public static int AccountInfoMonthCount { get; set; }
        public static Credentials Credentials { get; set; }
        public static Info Info { get; set; }
        public static AccountInfo AccountInfo { get; set; }
        public static string MainColor { get; set; }
        private static bool IsMainPageVisible { get; set; }
        private static float MainPageLogoOpacity { get; set; }
        private static bool IsStatisticPageVisible { get; set; }
	    private static float StatisticPageLogoOpacity { get; set; }
        private static bool IsPaymentPageVisible { get; set; }
        private static float PaymentPageLogoOpacity { get; set; }
        private static bool IsSettingsPageVisible { get; set; }
        private static float SettingsPageLogoOpacity { get; set; }
        
        static App()
        {
            CachingTimeInMinutes = 3;
            AccountInfoMonthCount = 4;
            Credentials = new Credentials();
	        Info = new Info();
            AccountInfo = new AccountInfo();
            ClearMenu();
            IsMainPageVisible = true;
            MainPageLogoOpacity = 1;
            MainColor = "#996600";
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
		        MainPage = new NavigationPage(new TabbedMainPage());
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
            AccountInfo = new AccountInfo();
            Credentials = new Credentials();
        }

        public static bool TryGetInfo()
        {
            return TryGetInfo(Credentials.Login, Credentials.Password);
        }

        public static bool TryGetInfo(string login, string password)
        {
            if (Info.RequestDate > DateTime.Now.AddMinutes(-CachingTimeInMinutes))
            {
                return true;
            }
            var info = new InfoService().GetInfo(login, password);
            if (string.IsNullOrWhiteSpace(info.ContractNumber))
            {
                return false;
            }
            Info = info;
            CrossSecureStorage.Current.SetValue(LoginKeyName, login);
            CrossSecureStorage.Current.SetValue("Password", password);
            Credentials.Login = login;
            Credentials.Password = password;

            return true;
        }

        public static void SetMainMenuActive()
	    {
	        ClearMenu();
	        IsMainPageVisible = true;
	        MainPageLogoOpacity = 1;
        }

	    private void MainButton_OnClicked(object sender, EventArgs e)
	    {
	        ClearMenu();
            IsMainPageVisible = true;
	        MainPageLogoOpacity = 1;
            MainPage = new NavigationPage(new MainPage());
        }

	    private void StatisticButton_OnClicked(object sender, EventArgs e)
	    {
	        ClearMenu();
	        IsStatisticPageVisible = true;
	        StatisticPageLogoOpacity = 1;
            MainPage = new NavigationPage(new StatisticPage());
	    }

	    private void PaymentButton_OnClicked(object sender, EventArgs e)
	    {
	        ClearMenu();
	        IsPaymentPageVisible = true;
	        PaymentPageLogoOpacity = 1;
            MainPage = new NavigationPage(new PaymentPage());
	    }

	    private void SettingsButton_OnClicked(object sender, EventArgs e)
	    {
	        ClearMenu();
	        IsSettingsPageVisible = true;
	        SettingsPageLogoOpacity = 1;
            MainPage = new NavigationPage(new SettingsPage());
        }

	    private static void ClearMenu()
	    {
	        IsMainPageVisible = false;
	        MainPageLogoOpacity = 0.3f;
	        IsStatisticPageVisible = false;
	        StatisticPageLogoOpacity = 0.3f;
	        IsPaymentPageVisible = false;
	        PaymentPageLogoOpacity = 0.3f;
	        IsSettingsPageVisible = false;
	        SettingsPageLogoOpacity = 0.3f;
        }
	}
}
