using System;
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

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            App.Exit();
            await Navigation.PushAsync(new AuthPage());
        }
    }
}