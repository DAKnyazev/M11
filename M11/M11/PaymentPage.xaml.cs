using M11.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using HtmlWebViewSource = Xamarin.Forms.HtmlWebViewSource;
using WebView = Xamarin.Forms.WebView;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaymentPage : BaseContentPage
    {
        private readonly InfoService _infoService;
        private string _paymentPage;
        private int _onNavigatedCount;
        private readonly WebView _browser;
        private ActivityIndicator LoadingIndicator { get; set; }

        public PaymentPage()
        {
            _infoService = new InfoService();
            _onNavigatedCount = 0;
            _browser = new WebView();
            _browser.Navigated += Browser_OnNavigated;
            LoadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex("#996600")
            };
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            LoadingIndicator.IsRunning = true;
            _browser.Source = new HtmlWebViewSource
            {
                Html = await _infoService.GetLoginPageContent(App.Credentials.Login, App.Credentials.Password, typeof(PaymentPage)),
            };
            _browser.IsVisible = false;
            PaymentLayout.Children.Add(LoadingIndicator,
                Constraint.RelativeToParent(parent => parent.Width * 0.425),
                Constraint.RelativeToParent(parent => parent.Height * 0.425),
                Constraint.RelativeToParent(parent => parent.Width * 0.15),
                Constraint.RelativeToParent(parent => parent.Height * 0.15));
            PaymentLayout.Children.Add(_browser,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height));

            _paymentPage = await _infoService.GetPaymentPageContent("3100910000000000052689", 100, "+79057503755", typeof(PaymentPage));
        }

        private async void Browser_OnNavigated(object sender, WebNavigatedEventArgs args)
        {
            if (_onNavigatedCount > 0)
            {
                if (args.Url.Contains(_infoService.BaseUrl))
                {
                    // Вернулись назад со страницы оплаты
                    App.SetMainMenuActive();
                    await Navigation.PushAsync(new MainPage());
                }
                return;
            }
            _browser.Source = new HtmlWebViewSource
            {
                Html = _paymentPage
            };
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            _browser.IsVisible = true;
            _onNavigatedCount++;
        }
    }
}