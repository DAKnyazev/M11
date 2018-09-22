using System.Linq;
using System.Text.RegularExpressions;
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

	        BalanceLabel.Text = App.Info.Balance;
	        TicketLayout.Children.Clear();
	        if (App.Info.Tickets.Any())
	        {
	            TicketLayout.Children.Add(new Label { Text = "Абонементы:", FontSize = 36, HorizontalTextAlignment = TextAlignment.Center });
            }

	        const int padding = 10;
	        foreach (var ticket in App.Info.Tickets)
	        {
	            var layout = new RelativeLayout();
	            layout.Children.Add(new BoxView { BackgroundColor = Color.FromHex("#F5F5DC") },
	                Constraint.Constant(padding),
	                Constraint.Constant(0),
	                Constraint.RelativeToParent(parent => parent.Width - 2 * padding),
	                Constraint.Constant(65));
	            var ticketDescriptions = ticket.Description.Split(',');
                layout.Children.Add(new Label
                    {
                        Text = ticketDescriptions.Length > 2 ? ticketDescriptions[2] : string.Empty,
                        FontSize = 24
                    },
                    Constraint.Constant(2 * padding),
                    null, 
                    Constraint.RelativeToParent(parent => parent.Width - 4 * padding),
                    Constraint.Constant(36));
	            var count = ticketDescriptions.Length > 0 ? Regex.Match(ticketDescriptions[0], @"\d+").Value : string.Empty;
	            var remainingCountText = string.IsNullOrWhiteSpace(count)
	                ? $"осталось поездок: {ticket.RemainingTripsCount}"
	                : $"осталось поездок: {ticket.RemainingTripsCount} (из {count})";

                layout.Children.Add(new Label { Text = $"{ticket.Status}, {remainingCountText}" },
	                Constraint.Constant(2 * padding),
	                Constraint.Constant(30),
	                Constraint.RelativeToParent(parent => parent.Width - 4 * padding),
	                Constraint.Constant(20));
                layout.Children.Add(new Label { Text = $"Использовать до: {ticket.ExpiryDate:dd.MM.yyyy HH:mm}" },
                    Constraint.Constant(2 * padding),
                    Constraint.Constant(50),
                    Constraint.RelativeToParent(parent => parent.Width - 4 * padding),
                    Constraint.Constant(20));
                
                TicketLayout.Children.Add(layout);
	        }
	    }
    }
}