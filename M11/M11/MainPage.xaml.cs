using System.Threading.Tasks;
using Xamarin.Forms;

namespace M11
{
	public partial class MainPage : BaseContentPage
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
            
	        BalanceLabel.FormattedText.Spans[0].Text = App.Info.Balance;
            BalanceLabel.FormattedText.Spans[1].Text = " Р";
            TicketLayout.Children.Clear();
            foreach (var ticket in App.Info.Tickets)
            {
                TicketLayout.Children.Add(new StackLayout
                {
                    Children =
                    {
                        new Label
                        {
                            Text = ticket.ContractNumber
                        },
                        new Label
                        {
                            Text = ticket.TransponderNumber
                        },
                        new Label
                        {
                            Text = ticket.Description
                        },
                        new Label
                        {
                            Text = ticket.StartDate.ToString("dd.MM.yyyy HH:mm")
                        },
                        new Label
                        {
                            Text = ticket.ExpiryDate.ToString("dd.MM.yyyy HH:mm")
                        },
                        new Label
                        {
                            Text = ticket.RemainingTripsCount.ToString()
                        },
                        new Label
                        {
                            Text = ticket.Status
                        },
                        new Label
                        {
                            Text = ticket.IsFairPriceOptionIncluded ? "Честная цена" : string.Empty
                        }
                    }
                });
            }

	        foreach (var link in App.Info.Links)
	        {
	            LinkLayout.Children.Add(
	                new Label
	                {
                        Text = link.Type + " " + link.RelativeUrl
	                });
	        }
        }
    }
}