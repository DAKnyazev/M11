using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
            var stack = new StackLayout();
            stack.Children.Add(new Label { Text = "Номер договора", FontSize = 30 });
            stack.Children.Add(new Label { Text = App.Info.ContractNumber, FontSize = 30 });
            stack.Children.Add(new Label { Text = "Статус", FontSize = 30 });
            stack.Children.Add(new Label { Text = App.Info.Status, FontSize = 30 });
            stack.Children.Add(new Label { Text = "Баланс", FontSize = 30 });
            stack.Children.Add(new Label { Text = App.Info.Balance, FontSize = 30 });
            Content = stack;
		}
	}
}