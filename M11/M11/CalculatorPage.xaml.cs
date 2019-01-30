using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalculatorPage : ContentPage
    {
        private readonly ActivityIndicator _loadingIndicator;

        public CalculatorPage()
        {
            InitializeComponent();
            this.Title = "Калькулятор";
            _loadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor)
            };
        }
    }
}