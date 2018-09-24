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
        public PaymentPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            Browser.Source = new HtmlWebViewSource
            {
                Html = await new InfoService().GetPaymentPageContent("3100910000000000052689", 100, "+79057503755",
                    typeof(PaymentPage))
            };
        }
    }
}