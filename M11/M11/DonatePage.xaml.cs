using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DonatePage : ContentPage
    {
        private const string DonateUrl = "https://money.yandex.ru/to/410016352760478";

        private WebView _browser;

        public DonatePage()
        {
            InitializeComponent();
            this.Title = "Благодарность разработчику";
        }

        protected override void OnAppearing()
        {
            DonateLayout.Children.Clear();
            _browser = new WebView
            {
                Source = new UrlWebViewSource
                {
                    Url = DonateUrl
                }
            };
            DonateLayout.Children.Add(_browser,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height));
        }
    }
}