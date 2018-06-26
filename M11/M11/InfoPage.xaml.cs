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
            stack.Children.Add(new Label { Text = "Номер договора" });
            stack.Children.Add(new Label { Text = info.ContractNumber });
            Content = stack;
		}
	}
}