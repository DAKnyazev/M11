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

        public PaymentPage()
        {
            _infoService = new InfoService();
            _onNavigatedCount = 0;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            Browser.Source = new HtmlWebViewSource
            {
                Html = await _infoService.GetLoginPageContent(App.Credentials.Login, App.Credentials.Password, typeof(PaymentPage))
            };

            _paymentPage = await _infoService.GetPaymentPageContent("3100910000000000052689", 100, "+79057503755", typeof(PaymentPage));
        }

        private void Browser_OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (_onNavigatedCount > 0)
            {
                return;
            }
            Browser.Source = new HtmlWebViewSource
            {
                Html = _paymentPage
            };
            _onNavigatedCount++;
        }
    }
}