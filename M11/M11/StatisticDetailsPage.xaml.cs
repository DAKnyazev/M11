using System.Linq;
using System.Threading.Tasks;
using M11.Common.Models.BillSummary;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatisticDetailsPage : ContentPage
    {
        private readonly ActivityIndicator _loadingIndicator;
        private readonly MonthBillSummary _monthBillSummary;

        public StatisticDetailsPage(MonthBillSummary monthBillSummary)
        {
            InitializeComponent();
            this.Title = $"{monthBillSummary.Period:MMMM} {monthBillSummary.Period.Year}";
            _monthBillSummary = monthBillSummary;
            _loadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor)
            };
        }

        protected override async void OnAppearing()
        {
            _loadingIndicator.IsRunning = true;
            DetailsLayout.Children.Clear();
            DetailsLayout.Children.Add(_loadingIndicator);
            await Task.Run(async () => await InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            const int padding = 10;
            App.FillGroups(_monthBillSummary);
            Device.BeginInvokeOnMainThread(() =>
            {
                _loadingIndicator.IsRunning = false;
                _loadingIndicator.IsVisible = false;
                var bills = _monthBillSummary
                    .Groups
                    .SelectMany(x => x.Bills)
                    .Distinct()
                    .OrderBy(x => x.Period);

                foreach (var billGroup in bills.GroupBy(x => x.Period.Date))
                {
                    var layout = new RelativeLayout();
                    var groupName = billGroup.Key.ToString("dd MMMM");
                    layout.Children.Add(new Label { Text = groupName, FontSize = 20 },
                        Constraint.Constant(padding * 3));
                    DetailsLayout.Children.Add(layout);
                    foreach (var bill in billGroup.OrderBy(x => x.Period).ThenByDescending(x =>
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
                        layout.Children.Add(new Image { Source = source },
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
                        layout.Children.Add(new BoxView { BackgroundColor = Color.LightGray, HeightRequest = 1 },
                            Constraint.Constant(64 + 2 * padding),
                            Constraint.Constant(64),
                            Constraint.RelativeToParent(x => x.Width - 64 - 3 * padding));
                        layout.Children.Add(new Label { Text = bill.Period.ToString("HH:mm"), FontSize = 14 },
                            Constraint.Constant(64 + 2 * padding),
                            Constraint.Constant(48));
                        DetailsLayout.Children.Add(layout);
                    }
                }
            });
        }
    }
}