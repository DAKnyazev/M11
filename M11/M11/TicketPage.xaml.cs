using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TicketPage : ContentPage
    {
        public event EventHandler<EventArgs> OnClosing;
        private readonly string _ticketLink;
        private bool _isLoginPageLoaded;
        private bool _isTicketPageLoaded;

        public TicketPage()
        {
            this.Title = "Абонемент";
            _ticketLink = App.AccountBalance?.TicketLink;
            InitializeComponent();
            LoadingIndicator.Color = Color.FromHex(App.MainColor);
        }

        protected override void OnAppearing()
        {
            if (_isLoginPageLoaded && !string.IsNullOrWhiteSpace(_ticketLink))
            {
                _isTicketPageLoaded = true;
                Browser.Source = new HtmlWebViewSource
                {
                    BaseUrl = _ticketLink
                };
                Browser.IsVisible = true;
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
            else
            {
                Browser.IsVisible = false;
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;
                Browser.Source = new HtmlWebViewSource
                {
                    Html = App.GetLoginPageContent(typeof(TicketPage))
                };
            }
        }

        protected override void OnDisappearing()
        {
            OnClosing?.Invoke(this, EventArgs.Empty);
        }

        private void BrowserOnNavigating(object sender, WebNavigatingEventArgs e)
        {
            if (_isLoginPageLoaded)
            {
                Browser.IsVisible = true;
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
            else
            {
                _isLoginPageLoaded = true;
            }
        }

        private void Browser_OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (_isTicketPageLoaded)
            {
                return;
            }

            _isTicketPageLoaded = true;
            Browser.Source = new UrlWebViewSource
            {
                Url = _ticketLink
            };
        }
    }
}