using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using M11.Common.Enums;
using M11.Services;
using Xamarin.Forms;

namespace M11
{
	public partial class MainPage : BaseContentPage
    {
        private ActivityIndicator LoadingIndicator { get; set; }
        private ActivityIndicator LastPaymentsIndicator { get; set; }

        public MainPage()
		{
			InitializeComponent();
		    LoadingIndicator = new ActivityIndicator
		    {
		        Color = Color.FromHex(App.MainColor)
		    };
		    LastPaymentsIndicator = new ActivityIndicator
		    {
		        Color = Color.FromHex(App.MainColor)
            };
		}

        protected override async void OnAppearing()
        {
            if (!App.IsNeedReloadMainPage)
            {
                return;
            }
            BalanceTitleLabel.IsVisible = false;
            BalanceCurrencyLabel.IsVisible = false;
            LastPaymentsLayout.IsVisible = false;
            LoadingIndicator.IsRunning = true;
            LastPaymentsIndicator.IsRunning = false;
            MainLayout.Children.Add(LoadingIndicator);
            await Task.Run(async () => await InitializeAsync());
	    }

        private async Task InitializeAsync()
	    {
	        if (!App.TryGetInfo())
	        {
	            Application.Current.MainPage = new AuthPage();
                return;
            }

	        Device.BeginInvokeOnMainThread(() =>
	        {
	            BalanceLabel.Text = App.AccountBalance.Balance + " ₽";
	            TicketLayout.Children.Clear();
	            if (App.AccountBalance.Tickets.Any())
	            {
	                TicketLayout.Children.Add(new Label
	                {
	                    Text = "Абонементы:",
	                    FontSize = 36,
	                    HorizontalTextAlignment = TextAlignment.Center
	                });
	            }

	            LastPaymentsLayout.Children.Clear();
	            LastPaymentsLayout.Children.Add(new Label
	            {
                    Text = "Последние траты:",
                    HorizontalOptions = LayoutOptions.Center,
                    FontFamily = "Bold,700",
                    FontSize = 18
                });
	        });

	        const int padding = 10;
	        foreach (var ticket in App.AccountBalance.Tickets)
	        {
	            var layout = new RelativeLayout();
	            layout.Children.Add(new BoxView { BackgroundColor = Color.FromHex("#F5F5DC") },
	                Constraint.Constant(padding),
	                Constraint.Constant(0),
	                Constraint.RelativeToParent(parent => parent.Width - 2 * padding),
	                Constraint.Constant(70));
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

	            Device.BeginInvokeOnMainThread(() =>
	            {
	                TicketLayout.Children.Add(layout);
                });
	        }
            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsRunning = false;
                BalanceTitleLabel.IsVisible = true;
                BalanceCurrencyLabel.IsVisible = true;
                LastPaymentsLayout.IsVisible = true;
                LastPaymentsIndicator.IsRunning = true;
                LastPaymentsIndicator.IsVisible = true;
                LastPaymentsLayout.Children.Add(LastPaymentsIndicator);
            });

            App.SetUpAccountInfo();

	        if (App.AccountInfo.BillSummaryList.Any())
	        {
	            var bills = App.GetLastBills();
                Device.BeginInvokeOnMainThread(() =>
                {
                    LastPaymentsIndicator.IsRunning = false;
                    LastPaymentsIndicator.IsVisible = false;

                    foreach (var bill in bills)
                    {
                        var layout = new RelativeLayout();
                        layout.Children.Add(new Label { Text = bill.Period.ToString("dd.MM.yyyy HH:mm") },
                            Constraint.Constant(padding),
                            Constraint.Constant(0),
                            Constraint.RelativeToParent(parent => parent.Width - 2 * padding),
                            Constraint.Constant(70));
                        layout.Children.Add(new Label { Text = bill.CostWithTax.ToString("0.00") + " ₽" },
                            Constraint.RelativeToParent(parent => padding + 3 * parent.Width / 4),
                            Constraint.Constant(0),
                            Constraint.RelativeToParent(parent => parent.Width / 4 - 2 * padding),
                            Constraint.Constant(70));
                        LastPaymentsLayout.Children.Add(layout);
                    }
                });
	        }
	    }
    }
}