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
            _loadingIndicator = new ActivityIndicator();
            _browser = new WebView();
            _browser.Navigating += BrowserOnNavigating;
            _browser.Source = MainPageUrl;
            _browser.EvaluateJavaScriptAsync("localStorage.setItem(\"m11-auth-token\", '\"\"');");
            MainPageLayout.Children.Clear();
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
                return;
            }

            await _browser.EvaluateJavaScriptAsync("localStorage.setItem(\"m11-auth-token\", '\"\"');");
            _browser.Source = MainPageUrl;
            _isTokenSet = true;
        }
    }
}