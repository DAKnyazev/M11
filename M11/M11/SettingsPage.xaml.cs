using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : BaseContentPage
    {
		public SettingsPage ()
		{
			InitializeComponent ();
		}

        private void Button_OnClicked(object sender, EventArgs e)
        {
            App.Exit();
            //await Navigation.PushAsync();
            Application.Current.MainPage = new AuthPage();
        }
    }
}