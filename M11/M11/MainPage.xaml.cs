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
	            TicketLayout.Children.Add(new Label { Text = "Абонементы:", FontSize = 36 });
            }
	        foreach (var ticket in App.Info.Tickets)
	        {
	            var layout = new RelativeLayout();
	            var ticketDescriptions = ticket.Description.Split(',');
                layout.Children.Add(new Label
                    {
                        Text = ticketDescriptions.Length > 2 ? ticketDescriptions[2] : string.Empty,
                        FontSize = 24
                    },
                    Constraint.RelativeToParent(parent => 0),
                    null, 
                    Constraint.RelativeToParent(parent => parent.Width),
                    Constraint.Constant(36));
	            var count = ticketDescriptions.Length > 0 ? Regex.Match(ticketDescriptions[0], @"\d+").Value : string.Empty;
	            var remainingCountText = string.IsNullOrWhiteSpace(count)
	                ? $"осталось поездок: {ticket.RemainingTripsCount}"
	                : $"осталось поездок: {ticket.RemainingTripsCount} (из {count})";

                layout.Children.Add(new Label { Text = $"{ticket.Status}, {remainingCountText}" },
	                Constraint.RelativeToParent(parent => 0),
	                Constraint.Constant(30),
	                Constraint.RelativeToParent(parent => parent.Width),
	                Constraint.Constant(20));
                layout.Children.Add(new Label { Text = $"Использовать до: {ticket.ExpiryDate:dd.MM.yyyy HH:mm}" },
                    Constraint.RelativeToParent(parent => 0),
                    Constraint.Constant(50),
                    Constraint.RelativeToParent(parent => parent.Width),
                    Constraint.Constant(20));
                layout.Children.Add(new Label { BackgroundColor = Color.Black },
                    Constraint.RelativeToParent(parent => 0),
                    Constraint.Constant(70),
                    Constraint.RelativeToParent(parent => parent.Width),
                    Constraint.Constant(1));
                
                TicketLayout.Children.Add(layout);
	        }
	    }
    }
}