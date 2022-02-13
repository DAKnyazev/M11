using Xamarin.Forms;

namespace M11.Base
{
    public abstract partial class BaseWebViewPage : BaseContentPage
    {
        private WebView _browser;
        private ActivityIndicator _loadingIndicator;
        private readonly string _url;
        private bool _isTokenSet = false;

        public BaseWebViewPage(string url)
        {
            _url = url;
        }

        protected void Setup()
        {
            _loadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor),
                IsRunning = true,
                IsVisible = true,
            };
            _browser = new ScrollableWebView();
            _browser.Navigating += BrowserOnNavigating;
            _browser.Source = _url;
            _browser.BackgroundColor = Color.AliceBlue;
            var layout = GetLayout();
            layout.Children.Clear();
            layout.Children.Add(
                _loadingIndicator,
                Constraint.RelativeToParent(parent => parent.Width * 0.425),
                Constraint.RelativeToParent(parent => parent.Height * 0.425),
                Constraint.RelativeToParent(parent => parent.Width * 0.15),
                Constraint.RelativeToParent(parent => parent.Height * 0.15));
            layout.Children.Add(
                _browser,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height));
        }

        protected abstract RelativeLayout GetLayout();

        private async void BrowserOnNavigating(object sender, WebNavigatingEventArgs args)
        {
            if (_isTokenSet)
            {
                _loadingIndicator.IsRunning = false;
                _loadingIndicator.IsVisible = false;
                return;
            }

            await _browser.EvaluateJavaScriptAsync($"localStorage.setItem(\"m11-auth-token\", '\"{App.Credentials.Token}\"');");
            _browser.Source = _url;
            _isTokenSet = true;
        }
    }
}
