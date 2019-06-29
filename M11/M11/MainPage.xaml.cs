using System;
using System.Linq;
using System.Net;
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
            if (!App.IsNeedReloadMainPage && !string.IsNullOrWhiteSpace(BalanceLabel.Text) && BalanceLabel.Text == App.AccountBalance.Balance + " ₽")
            {
                return;
            }
            BalanceTitleLabel.IsVisible = !string.IsNullOrWhiteSpace(BalanceLabel.Text);
            LastPaymentsLayout.IsVisible = !string.IsNullOrWhiteSpace(BalanceLabel.Text);
            //BalanceLabel.Text = "";
            LoadingIndicator.IsRunning = true;
            LastPaymentsIndicator.IsRunning = false;
            MainLayout.Children.Add(LoadingIndicator);
            await Task.Run(async () => await InitializeAsync());
	    }

        private async Task InitializeAsync()
	    {
	        InitializeBalanceAndTickets();
	        InitializeLastBills();
	    }

        private void InitializeBalanceAndTickets()
        {
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
                    Text = "Свежие траты:",
                    HorizontalOptions = LayoutOptions.Center,
                    FontFamily = "Bold,700",
                    FontSize = 18
                });
                LastPaymentsLayout.Children.Add(new Label
                {
                    Text = "(долгая загрузка)",
                    FontSize = 10,
                    HorizontalTextAlignment = TextAlignment.Center
                });
            });

            try
            {
                const int padding = 10;
                foreach (var ticket in App.AccountBalance.Tickets)
                {
                    var layout = new RelativeLayout();
                    layout.Children.Add(new BoxView {BackgroundColor = Color.FromHex("#F5F5DC")},
                        Constraint.Constant(padding),
                        Constraint.Constant(0),
                        Constraint.RelativeToParent(parent => parent.Width - 2 * padding),
                        Constraint.Constant(70));
                    layout.Children.Add(new Label
                        {
                            Text = ticket.ShortDescription,
                            FontSize = 24
                        },
                        Constraint.Constant(2 * padding),
                        null,
                        Constraint.RelativeToParent(parent => parent.Width - 4 * padding),
                        Constraint.Constant(36));

                    var remainingCountText = string.IsNullOrWhiteSpace(ticket.TotalTripsCount)
                        ? $"осталось поездок: {ticket.RemainingTripsCount}"
                        : $"осталось поездок: {ticket.RemainingTripsCount} (из {ticket.TotalTripsCount})";

                    layout.Children.Add(new Label {Text = $"{ticket.Status}, {remainingCountText}"},
                        Constraint.Constant(2 * padding),
                        Constraint.Constant(30),
                        Constraint.RelativeToParent(parent => parent.Width - 4 * padding),
                        Constraint.Constant(20));
                    layout.Children.Add(new Label {Text = $"Использовать до: {ticket.ExpiryDate:dd.MM.yyyy HH:mm}"},
                        Constraint.Constant(2 * padding),
                        Constraint.Constant(50),
                        Constraint.RelativeToParent(parent => parent.Width - 4 * padding),
                        Constraint.Constant(20));

                    Device.BeginInvokeOnMainThread(() => { TicketLayout.Children.Add(layout); });
                }
            }
            catch
            {
                // Чтобы работал остальной функционал
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsRunning = false;
                BalanceTitleLabel.IsVisible = true;
                LastPaymentsLayout.IsVisible = true;
                LastPaymentsIndicator.IsRunning = true;
                LastPaymentsIndicator.IsVisible = true;
                LastPaymentsLayout.Children.Add(LastPaymentsIndicator);
            });
        }

        private void InitializeLastBills()
        {
            try
            {
                const int padding = 10;
                if (!App.AccountInfo.BillSummaryList.Any())
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        LastPaymentsIndicator.IsRunning = false;
                        LastPaymentsIndicator.IsVisible = false;
                    });

                    return;
                }
                var bills = App.GetLastBills();
                Device.BeginInvokeOnMainThread(() =>
                {
                    LastPaymentsIndicator.IsRunning = false;
                    LastPaymentsIndicator.IsVisible = false;

                    foreach (var billGroup in bills.GroupBy(x => x.Period.Date))
                    {
                        var layout = new RelativeLayout();
                        var groupName = billGroup.Key.Date == DateTime.Now.Date
                            ? "Сегодня"
                            : billGroup.Key.Date == DateTime.Now.AddDays(-1).Date
                                ? "Вчера"
                                : billGroup.Key.ToString("dd MMMM");
                        layout.Children.Add(new Label {Text = groupName, FontSize = 20},
                            Constraint.Constant(padding * 3));
                        LastPaymentsLayout.Children.Add(layout);
                        foreach (var bill in billGroup.OrderByDescending(x => x.Period).ThenByDescending(x =>
                            (x.EntryPoint?.Contains("11") ?? false) && (x.EntryPoint?.Contains("58") ?? false)
                                ? 0
                                : 1))
                        {
                            layout = new RelativeLayout();
                            var source = bill.IsServicePay
                                ? ImageSource.FromFile("service.png")
                                : bill.CostWithTax == 0
                                    ? ImageSource.FromFile("ticket.png")
                                    : ImageSource.FromFile("wallet.png");
                            layout.Children.Add(new Image {Source = source},
                                Constraint.Constant(padding),
                                Constraint.Constant(0),
                                Constraint.Constant(64),
                                Constraint.Constant(64));
                            layout.Children.Add(new Label
                                {
                                    Text = bill.IsServicePay
                                        ? "Ежемесячный платеж"
                                        : bill.IsTicketBuy
                                            ? App.GetTicketDescription(bill.PAN)
                                            : $"{App.GetPointName(bill.EntryPoint)} -> {(bill.EntryPoint?.Length + bill.ExitPoint?.Length > 30 ? "\r\n -> " : "")}{App.GetPointName(bill.ExitPoint)}",
                                    FontFamily = "Bold,700",
                                    FontSize = 14
                                },
                                Constraint.Constant(64 + 2 * padding),
                                Constraint.Constant(10));
                            layout.Children.Add(new Label
                                {
                                    Text = bill.CostWithTax != 0
                                        ? bill.CostWithTax.ToString("0") + " ₽"
                                        : bill.Amount.ToString("0") + (bill.IsTicketBuy ? " ₽" : ""),
                                    FontSize = 34,
                                    FontFamily = "Bold,700",
                                    WidthRequest = 200,
                                    HorizontalTextAlignment = TextAlignment.End
                                },
                                Constraint.RelativeToParent(x => x.Width - 2 * padding - 200),
                                Constraint.Constant(22));
                            layout.Children.Add(new BoxView {BackgroundColor = Color.LightGray, HeightRequest = 1},
                                Constraint.Constant(64 + 2 * padding),
                                Constraint.Constant(64),
                                Constraint.RelativeToParent(x => x.Width - 64 - 3 * padding));
                            layout.Children.Add(new Label {Text = bill.Period.ToString("HH:mm"), FontSize = 14},
                                Constraint.Constant(64 + 2 * padding),
                                Constraint.Constant(48));
                            LastPaymentsLayout.Children.Add(layout);
                        }
                    }
                });
            }
            catch
            {
                // Чтобы работал остальной функционал
            }
        }

        /// <summary>
        /// Открыть калькулятор
        /// </summary>
        private async void OpenCalculator(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new CalculatorPage()));
        }

        /// <summary>
        /// Открыть страницу покупки абонемента
        /// </summary>
        private async void OpenTicketPage(object sender, EventArgs e)
        {
            var ticketPage = new TicketPage();
            ticketPage.OnClosing += (o, args) => { App.SetNeedReload(); OnAppearing(); };
            await Navigation.PushModalAsync(new NavigationPage(ticketPage));
        }
    }
}