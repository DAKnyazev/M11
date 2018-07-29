using System.Globalization;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace M11
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
        }

	    protected override async void OnAppearing()
	    {
	        await InitializeAsync();
	    }


        private async Task InitializeAsync()
	    {
	        if (!await App.TryGetInfo())
	        {
	            await Navigation.PushAsync(new AuthPage());
                return;
            }

	        //ContractNumberLabel.Text = App.Info.ContractNumber;
	        BalanceLabel.FormattedText.Spans[0].Text = App.Info.Balance;
            BalanceLabel.FormattedText.Spans[1].Text = " Р";
	        //StatusLabel.Text = App.Info.Status;
	    }
    }
}