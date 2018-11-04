using System;
using System.Linq;
using System.Threading;
using M11.Common.Enums;
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
        private int _onNavigatedCount;
        private WebView _browser;
        private ActivityIndicator LoadingIndicator { get; set; }
        private string _paymentPageUrl = "securepayments.sberbank.ru";
        private bool _isNeedReload;
        private bool _isPaymentPageWasOpened;

        public PaymentPage()
        {
            _infoService = new InfoService();
            _isNeedReload = true;
            InitializeComponent();
        }

        private void Init()
        {
            _onNavigatedCount = 0;
            _browser = new WebView();
            _browser.Navigated += Browser_OnNavigated;
            _browser.Navigating += BrowserOnNavigating;
            LoadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor)
            };
        }

        protected override void OnAppearing()
        {
            if (_isPaymentPageWasOpened)
            {
                _isNeedReload = false;
            }
            if (!_isNeedReload)
            {
                return;
            }
            PaymentLayout.Children.Clear();
            Init();
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            _browser.IsVisible = false;
            _browser.Source = new HtmlWebViewSource
            {
                Html = _infoService.GetLoginPageContent(App.Credentials.Login, App.Credentials.Password, typeof(PaymentPage)),
            };
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
        }

        private void Browser_OnNavigated(object sender, WebNavigatedEventArgs args)
        {
            if (args.Url.Contains(_paymentPageUrl))
            {
                _isPaymentPageWasOpened = true;
            }
            if (_onNavigatedCount > 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(App.AccountInfo?.AccountId))
            {
                App.AccountInfo = _infoService.GetAccountInfo(
                    App.Info.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                    App.Info.CookieContainer,
                    DateTime.Now,
                    DateTime.Now.AddMonths(-App.AccountInfoMonthCount));
            }
            _browser.Source = new HtmlWebViewSource
            {
                Html = _infoService.GetPaymentPageContent(App.AccountInfo.AccountId, 100, App.Info.Phone, typeof(PaymentPage))
            };
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            _browser.IsVisible = true;
            _isNeedReload = false;
            _onNavigatedCount++;
        }

        private void BrowserOnNavigating(object sender, WebNavigatingEventArgs args)
        {
            _isNeedReload = true;
            if (_onNavigatedCount <= 0 || !args.Url.Contains(_infoService.BaseUrl))
            {
                return;
            }

            // Вернулись назад со страницы оплаты (нужно перезагрузить информацию о счете)
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            _browser.IsVisible = false;
            App.Info.RequestDate = DateTime.MinValue;
            App.AccountInfo.RequestDate = DateTime.MinValue;
            Application.Current.MainPage = new TabbedMainPage();
        }
    }
}