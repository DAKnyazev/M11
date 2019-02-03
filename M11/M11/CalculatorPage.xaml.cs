using System;
using System.Threading.Tasks;
using M11.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalculatorPage : ContentPage
    {
        private readonly ActivityIndicator _loadingIndicator;
        private readonly CalculatorService _calculatorService;

        public CalculatorPage()
        {
            InitializeComponent();
            _calculatorService = new CalculatorService();
            this.Title = "Калькулятор";
            _loadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor)
            };
            
        }

        protected override async void OnAppearing()
        {
            ErrorLabel.IsVisible = false;
            _loadingIndicator.IsRunning = true;
            CashCostLabel.IsVisible = false;
            TransponderCostLabel.IsVisible = false;
            await Task.Run(async () => await InitializeAsync());
        }

        private void Calculate(object sender, EventArgs e)
        {
            var result = _calculatorService.Calculate(1);
            CashCostLabel.Text = result.CashCost.ToString("0");
            TransponderCostLabel.Text = result.TransponderCost.ToString("0");
            CashCostLabel.IsVisible = true;
            TransponderCostLabel.IsVisible = true;
        }

        private async Task InitializeAsync()
        {
            var isLoaded = await _calculatorService.TryLoadAsync();
            if (isLoaded)
            {
                Device.BeginInvokeOnMainThread(() => { _loadingIndicator.IsRunning = false; });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ErrorLabel.IsVisible = true;
                    CalculatorGrid.IsVisible = false;
                });
            }
        }
    }
}