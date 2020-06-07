using System;
using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<string, string> DepartureDictionary { get; set; }
        private Dictionary<string, string> DestinationDictionary { get; set; }
        private string DayWeek { get; set; }
        private string Time { get; set; }
        private Dictionary<string, string> CategoryDictionary { get; set; }

        public CalculatorPage()
        {
            CategoryDictionary = new Dictionary<string, string>()
            {
                {"Группа 1", "1"},
                {"Группа 2", "2"},
                {"Группа 3", "3"},
                {"Группа 4", "4"}
            };
            InitializeComponent();
            _calculatorService = new CalculatorService();
            this.Title = "Калькулятор";
            _loadingIndicator = new ActivityIndicator
            {
                Color = Color.FromHex(App.MainColor)
            };
            CalculatorLayout.Children.Add(_loadingIndicator);
        }

        protected override async void OnAppearing()
        {
            ErrorLabel.IsVisible = false;
            _loadingIndicator.IsEnabled = true;
            _loadingIndicator.IsRunning = true;
            CalculatorGrid.IsVisible = false;
            SwapButton.Text = "><";
            await Task.Run(async () => await InitializeAsync());
        }

        private void Calculate(object sender, EventArgs e)
        {
            Calculate();
        }

        private void Calculate()
        {
            if (DeparturePicker.SelectedItem == null
                || DestinationPicker.SelectedItem == null
                || string.IsNullOrEmpty(DayWeek)
                || string.IsNullOrEmpty(Time)
                || CategoryPicker.SelectedItem == null)
            {
                return;
            }

            var result = _calculatorService.Calculate(
                CategoryDictionary[CategoryPicker.SelectedItem.ToString()],
                DayWeek,
                Time,
                DepartureDictionary[DeparturePicker.SelectedItem.ToString()],
                DestinationDictionary[DestinationPicker.SelectedItem.ToString()]);
            DetourLabel.IsVisible = result.IsComposite;
            CashCostLabel.Text = result.CashCost.ToString("0");
            TransponderCostLabel.Text = result.TransponderCost.ToString("0");
            if (App.AutodorCalculatorPrice > 0)
            {
                CashCostLabel.Text += $" (+{App.AutodorCalculatorPrice} = {(result.CashCost + App.AutodorCalculatorPrice).ToString("0")})";
                TransponderCostLabel.Text += $" (+{App.AutodorCalculatorPrice} = {(result.TransponderCost + App.AutodorCalculatorPrice).ToString("0")})";
            }

            CashCostLabel.Text += " ₽";
            TransponderCostLabel.Text += " ₽";
        }

        private async Task InitializeAsync()
        {
            var isLoaded = await _calculatorService.TryLoadAsync();
            if (isLoaded)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DestinationDictionary = _calculatorService.GetDestinations();
                    DepartureDictionary = _calculatorService.GetDepartures();
                    DestinationPicker.ItemsSource = DepartureDictionary.Select(x => x.Key).ToList();
                    DeparturePicker.ItemsSource = DepartureDictionary.Select(x => x.Key).ToList();
                    DestinationPicker.SelectedItem = DestinationDictionary
                        .First(x => x.Key.ToLower().Contains("москва")).Key;
                    DeparturePicker.SelectedItem = DepartureDictionary
                        .First(x => x.Key.ToLower().Contains("зеленоград")).Key;
                    CategoryPicker.ItemsSource = CategoryDictionary.Select(x => x.Key).ToList();
                    CategoryPicker.SelectedItem = CategoryDictionary.First().Key;
                    SetButtonsClassId();
                    FillTimeLayout();
                    SetStartDateTime();
                    _loadingIndicator.IsRunning = false;
                    CalculatorGrid.IsVisible = true;
                });
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

        private void SetStartDateTime()
        {
            var weekday = DateTime.Now.DayOfWeek != DayOfWeek.Sunday ? (int) DateTime.Now.DayOfWeek : 7;
            var index = 0;
            foreach (var item in CalculatorGrid.Children.Where(c => Grid.GetRow(c) == 2 && Grid.GetColumn(c) % 2 == 1))
            {
                if (item is Button button)
                {
                    index++;
                    if (index == weekday)
                    {
                        DayWeek = button.ClassId;
                        button.BorderWidth = 4;
                        break;
                    }
                }
            }

            Calculate();
        }

        private void DayWeek_OnClicked(object sender, EventArgs e)
        {
            ClearDayWeekButtonBorders();
            if (sender is Button button)
            {
                DayWeek = button.ClassId;
                button.BorderWidth = 4;
            }
            Calculate(sender, e);
        }

        private void Time_OnClicked(object sender, EventArgs e)
        {
            ClearTimeButtonBorders();
            if (sender is Button button)
            {
                Time = button.ClassId;
                button.BorderWidth = 4;
            }
            Calculate(sender, e);
        }

        private void ClearDayWeekButtonBorders()
        {
            foreach (var item in CalculatorGrid.Children.Where(c => Grid.GetRow(c) == 2 && Grid.GetColumn(c) % 2 == 1))
            {
                if (item is Button button)
                {
                    button.BorderWidth = 0;
                }
            }
        }

        private void ClearTimeButtonBorders()
        {
            foreach (var item in TimeLayout.Children)
            {
                if (item is Button button)
                {
                    button.BorderWidth = 0;
                }
            }
        }

        private void SetButtonsClassId()
        {
            var dayweeks = _calculatorService.GetDayWeeks();
            var index = 0;
            foreach (var item in CalculatorGrid.Children.Where(c => Grid.GetRow(c) == 2 && Grid.GetColumn(c) % 2 == 1))
            {
                if (item is Button button)
                {
                    button.ClassId = dayweeks[index++];
                }
            }
        }

        private void FillTimeLayout()
        {
            TimeLayout.Children.Clear();
            foreach (var times in _calculatorService.GetTimes())
            {
                var isCurrent = times.Item1 <= DateTime.Now.Hour && times.Item2 > DateTime.Now.Hour;
                var text = $"{times.Item1}-{times.Item2}";
                var button = new Button
                {
                    ClassId = times.Item3,
                    Text = text,
                    BorderColor = Color.FromHex("ed1c24"),
                    WidthRequest = text.Length == 3 ? 50 : (text.Length == 4 ? 60 : 65),
                    BackgroundColor = Color.FromHex("F5F5DC"),
                    BorderWidth = isCurrent ? 4 : 0
                };
                Time = times.Item3;
                TimeLayout.Children.Add(button);
                button.Clicked += Time_OnClicked;
            }
        }

        private void SwapDepartureAndDestination(object sender, EventArgs e)
        {
            var destination = DestinationPicker.SelectedItem;
            DestinationPicker.SelectedItem = DeparturePicker.SelectedItem;
            DeparturePicker.SelectedItem = destination;
        }

        private async void OpenAutodorCalculator(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new AutodorCalculatorPage()));
        }
    }
}