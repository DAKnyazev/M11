using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AutodorCalculatorPage : ContentPage
    {
        private const string AutodorCalculatorUrl = "https://tpass.me/trassy/m11-sankt-peterburg/";

        private WebView _browser;        

        public AutodorCalculatorPage()
        {
            InitializeComponent();
            this.Title = "Калькулятор Автодор";
        }

        protected override void OnAppearing()
        {
            AutodorCalculatorLayout.Children.Clear();            
            _browser = new WebView
            {
                Source = new UrlWebViewSource
                {
                    Url = AutodorCalculatorUrl
                }
            };
            AutodorCalculatorLayout.Children.Add(_browser,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height));
        }

        protected async override void OnDisappearing()
        {
            var priceString = await _browser.EvaluateJavaScriptAsync("document.getElementsByClassName('price')[0].innerText");
            try
            {
                if (decimal.TryParse(priceString.Split(' ')[0], out var price))
                {
                    App.AutodorCalculatorPrice = price;
                }
                else
                {
                    App.AutodorCalculatorPrice = 0;
                }
            }
            catch
            {
                App.AutodorCalculatorPrice = 0;
            }
        }
    }
}