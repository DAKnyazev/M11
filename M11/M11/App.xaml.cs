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
        public static Credentials Credentials { get; set; }
        public static Info Info { get; set; }

	    static App()
	    {
	        Credentials = new Credentials();
	        Info = new Info();
        }

        public App()
		{
			InitializeComponent();

		    Credentials.Login = CrossSecureStorage.Current.HasKey("Login") ? CrossSecureStorage.Current.GetValue("Login") : string.Empty;
		    Credentials.Password = CrossSecureStorage.Current.HasKey("Password") ? CrossSecureStorage.Current.GetValue("Password") : string.Empty;
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

	    public static async Task<bool> TryGetInfo()
	    {
	        return await TryGetInfo(Credentials.Login, Credentials.Password);
	    }

	    public static async Task<bool> TryGetInfo(string login, string password)
	    {
	        var info = await new AuthService().GetParticipantInfo(login, password);
	        if (!string.IsNullOrWhiteSpace(info.ContractNumber))
	        {
	            Info = info;
	            CrossSecureStorage.Current.SetValue("Login", login);
	            CrossSecureStorage.Current.SetValue("Password", password);
                Credentials.Login = login;
	            Credentials.Password = password;

	            return true;
	        }

	        return false;
	    }
    }
}
