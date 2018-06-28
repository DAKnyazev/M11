using System.Threading.Tasks;
using M11.Common.Models;
using M11.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
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
		    Credentials.Login = App.Current.Properties.ContainsKey("Login") ? App.Current.Properties["Login"].ToString() : string.Empty;
		    Credentials.Password = App.Current.Properties.ContainsKey("Password") ? App.Current.Properties["Password"].ToString() : string.Empty;
		    if (string.IsNullOrWhiteSpace(Credentials.Login) || string.IsNullOrWhiteSpace(Credentials.Password))
		    {
		        MainPage = new NavigationPage(new AuthPage());
            }
		    else
		    {
		        MainPage = new NavigationPage(new MainPage());
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

	    public static async Task<bool> TryGetInfo(string login, string password)
	    {
	        var info = await new AuthService().GetParticipantInfo(login, password);
	        if (!string.IsNullOrWhiteSpace(info.ContractNumber))
	        {
	            Info = info;
	            Credentials.Login = login;
	            Credentials.Password = password;

	            return true;
	        }

	        return false;
	    }
	}
}
