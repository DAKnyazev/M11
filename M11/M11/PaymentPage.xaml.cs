using System;
using System.Linq;
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
        private int _onNavigatedToBaseUrlCount;
        private readonly WebView _browser;
        private ActivityIndicator LoadingIndicator { get; set; }

        public PaymentPage()
        {
            _infoService = new InfoService();
            _onNavigatedCount = 0;
            _onNavigatedToBaseUrlCount = 0;
            _browser = new WebView();
            _browser.Navigated += Browser_OnNavigated;
            LoadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor)
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
        }

        private async void Browser_OnNavigated(object sender, WebNavigatedEventArgs args)
        {
            if (_onNavigatedCount > 0)
            {
                if (args.Url.Contains(_infoService.BaseUrl))
                {
                    if (_onNavigatedToBaseUrlCount > 0)
                    {
                        // Вернулись назад со страницы оплаты (нужно перезагрузить информацию о счете)
                        App.SetMainMenuActive();
                        App.Info.RequestDate = DateTime.MinValue;
                        App.AccountInfo.RequestDate = DateTime.MinValue;
                        await Navigation.PushAsync(new MainPage());
                    }

                    _onNavigatedToBaseUrlCount++;
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(App.AccountInfo?.AccountId))
            {
                App.AccountInfo = await _infoService.GetAccountInfo(
                    App.Info.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                    App.Info.CookieContainer,
                    DateTime.Now,
                    DateTime.Now.AddMonths(-App.AccountInfoMonthCount));
            }
            _browser.Source = new HtmlWebViewSource
            {
                Html = await _infoService.GetPaymentPageContent(App.AccountInfo.AccountId, 100, App.Info.Phone, typeof(PaymentPage))
            };
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            _browser.IsVisible = true;
            _onNavigatedCount++;
        }
    }
}