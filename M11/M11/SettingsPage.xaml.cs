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
			InitializeComponent();
		    //NotificationsSwitch.IsToggled = App.IsNotificationsOn;
		}

        private void Button_OnClicked(object sender, EventArgs e)
        {
            App.Exit();
        }

        private void Switch_OnToggled(object sender, ToggledEventArgs e)
        {
            App.IsNotificationsOn = e.Value;
        }

        private void SwitchTurbo_OnToggled(object sender, ToggledEventArgs e)
        {
            App.IsNotificationsTurboOn = e.Value;
        }
    }
}