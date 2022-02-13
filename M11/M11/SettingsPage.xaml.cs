using System;
using M11.Common.Extentions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : BaseContentPage
    {
		public SettingsPage()
		{
			InitializeComponent();
		    NotificationFrequencyPicker.ItemsSource = App.NotificationFrequenciesDescriptions;
		    NotificationFrequencyPicker.SelectedItem = App.NotificationFrequency.GetDescription();
		    NotificationFrequencyDescriptionLabel.Text = App.NotificationFrequency.GetFullDescription();
        }

        protected override void OnAppearing()
        {
            App.ClearSettingsBadge();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            App.Exit();
        }

        private void NotificationFrequencyPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            App.NotificationFrequency = App.NotificationFrequencies[NotificationFrequencyPicker.SelectedIndex];
            NotificationFrequencyDescriptionLabel.Text =
                App.NotificationFrequenciesFullDescriptions[NotificationFrequencyPicker.SelectedIndex];
        }

        private async void DonateButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new DonatePage()));
        }

        private async void OpenCalculator(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new CalculatorPage()));
        }
    }
}