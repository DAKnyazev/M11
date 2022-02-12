using M11.Base;
using Xamarin.Forms;

namespace M11
{
    public partial class MainPage : BaseContentPage
    {
        private const string MainPageUrl = "https://lk.m11-neva.ru/";

        private readonly WebView _browser;
        private readonly ActivityIndicator _loadingIndicator;

        private bool _isTokenSet = false;

        public MainPage()
        {
            InitializeComponent();
            _loadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor),
                IsRunning = true,
                IsVisible = true,
            };
            _browser = new ScrollableWebView();
            _browser.Navigating += BrowserOnNavigating;
            _browser.Source = MainPageUrl;
            _browser.BackgroundColor = Color.AliceBlue;
            MainPageLayout.Children.Clear();
            MainPageLayout.Children.Add(
                _loadingIndicator,
                Constraint.RelativeToParent(parent => parent.Width * 0.425),
                Constraint.RelativeToParent(parent => parent.Height * 0.425),
                Constraint.RelativeToParent(parent => parent.Width * 0.15),
                Constraint.RelativeToParent(parent => parent.Height * 0.15));
            MainPageLayout.Children.Add(
                _browser,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height));
        }

        protected override void OnAppearing()
        {
        }

        private async void BrowserOnNavigating(object sender, WebNavigatingEventArgs args)
        {
            if (_isTokenSet)
            {
                _loadingIndicator.IsRunning = false;
                _loadingIndicator.IsVisible = false;
                return;
            }

            await _browser.EvaluateJavaScriptAsync($"localStorage.setItem(\"m11-auth-token\", '\"{App.Credentials.Token}\"');");
            _browser.Source = MainPageUrl;
            _isTokenSet = true;
        }
    }
}