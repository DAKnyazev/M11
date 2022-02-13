using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalculatorPage : ContentPage
    {
        private const string CalculatorUrl = "https://m11-neva.ru/trip/calc/";

        private WebView _browser;

        public CalculatorPage()
        {
            InitializeComponent();
            this.Title = "Расчет путешествия";
        }

        protected override void OnAppearing()
        {
            CalculatorLayout.Children.Clear();
            _browser = new WebView
            {
                Source = new UrlWebViewSource
                {
                    Url = CalculatorUrl
                }
            };
            CalculatorLayout.Children.Add(_browser,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height));
        }
    }
}