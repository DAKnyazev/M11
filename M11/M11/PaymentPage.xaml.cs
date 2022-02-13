using System;
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
        private WebView _browser;
        private ActivityIndicator LoadingIndicator { get; set; }
        private string _paymentPageUrl = "securepayments.sberbank.ru";
        private string _paymentPageUrlPath = "sberbank";
        private bool _isNeedReload;
        private PaymentPageStep _currentStep = PaymentPageStep.None;

        public PaymentPage()
        {
            _infoService = new InfoService();
            _isNeedReload = true;
            InitializeComponent();
        }

        private void Init()
        {
            _browser = new WebView();
            _browser.Navigating += BrowserOnNavigating;
            LoadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor)
            };
        }

        protected override void OnAppearing()
        {
            if (_currentStep >= PaymentPageStep.MyPaymentPage)
            {
                return;
            }
            PaymentLayout.Children.Clear();
            Init();
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            RenderLoginPage();
            PaymentLayout.Children.Add(LoadingIndicator,
                Constraint.RelativeToParent(parent => parent.Width * 0.425),
                Constraint.RelativeToParent(parent => parent.Height * 0.425),
                Constraint.RelativeToParent(parent => parent.Width * 0.15),
                Constraint.RelativeToParent(parent => parent.Height * 0.15));
            PaymentLayout.Children.Add(_browser,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height * 1.5));
        }

        private void BrowserOnNavigating(object sender, WebNavigatingEventArgs args)
        {
            switch (_currentStep)
            {
                case PaymentPageStep.LoginPage:
                    RenderMyPaymentPage();
                    return;
                case PaymentPageStep.MyPaymentPage:
                    _currentStep = PaymentPageStep.SberbankCardPage;
                    return;
                case PaymentPageStep.SberbankCardPage:
                    _currentStep = PaymentPageStep.ClientBankSmsPage;
                    return;
                case PaymentPageStep.ClientBankSmsPage:
                    _currentStep = PaymentPageStep.ReturnFromClientBankPage;
                    break;
            }

            if (args.Url.Contains(_infoService.BaseUrl))
            {
                GoToMainPage();
            }
        }

        private void RenderLoginPage()
        {
            _browser.IsVisible = false;
            _browser.Source = new HtmlWebViewSource
            {
                Html = App.GetLoginPageContent(typeof(PaymentPage))
            };
            _currentStep = PaymentPageStep.LoginPage;
        }

        private void RenderMyPaymentPage()
        {
            _browser.Source = new HtmlWebViewSource
            {
                Html = App.GetPaymentPageContent(typeof(PaymentPage))
            };
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            _browser.IsVisible = true;
            _isNeedReload = false;
            _currentStep = PaymentPageStep.MyPaymentPage;
        }

        private void GoToMainPage()
        {
            // Вернулись назад со страницы оплаты (нужно перезагрузить информацию о счете)
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            Thread.Sleep(3000);
            _browser.IsVisible = false;
            App.AccountBalance.RequestDate = DateTime.MinValue;
            App.GoToMainPage(true);
            _currentStep = PaymentPageStep.None;
        }
    }
}