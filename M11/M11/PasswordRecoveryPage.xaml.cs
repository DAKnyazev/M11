using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PasswordRecoveryPage : ContentPage
    {
        private const string PasswordRecoveryUrl = "https://private.m11-neva.ru/onyma/system/password/recover/";

        private WebView _browser;

        public PasswordRecoveryPage()
        {
            InitializeComponent();
            this.Title = "Восстановление пароля";
        }

        protected override void OnAppearing()
        {
            PasswordRecoveryLayout.Children.Clear();
            _browser = new WebView
            {
                Source = new UrlWebViewSource
                {
                    Url = PasswordRecoveryUrl
                }
            };
            PasswordRecoveryLayout.Children.Add(_browser,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height));
        }
    }
}