using M11.Common.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace M11
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class InfoPage : ContentPage
	{
		public InfoPage(Info info)
		{
			InitializeComponent();
            var stack = new StackLayout();
            stack.Children.Add(new Label { Text = "Номер договора", FontSize = 30 });
            stack.Children.Add(new Label { Text = info.ContractNumber, FontSize = 30 });
            stack.Children.Add(new Label { Text = "Статус", FontSize = 30 });
            stack.Children.Add(new Label { Text = info.Status, FontSize = 30 });
            stack.Children.Add(new Label { Text = "Баланс", FontSize = 30 });
            stack.Children.Add(new Label { Text = info.Balance, FontSize = 30 });
            Content = stack;
		}
	}
}