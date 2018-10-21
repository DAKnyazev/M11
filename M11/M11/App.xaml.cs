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
        
        static App()
        {
            CachingTimeInMinutes = 3;
            AccountInfoMonthCount = 4;
            Credentials = new Credentials();
	        Info = new Info();
            AccountInfo = new AccountInfo();
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
	        CrossSecureStorage.Current.SetValue(LoginKeyName, string.Empty);
	        CrossSecureStorage.Current.SetValue(PasswordKeyName, string.Empty);
            Info = new Info();
            AccountInfo = new AccountInfo();
            Credentials = new Credentials();
	        Current.MainPage = new AuthPage();
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
	}
}
