using M11.Services;
using System;
using Xamarin.Forms;

namespace M11
{
	public partial class MainPage : ContentPage
	{
        private readonly Label _loginLabel;
        private readonly Entry _loginEntry;
        private readonly Label _passwordLabel;
        private readonly Entry _passwordEntry;
        private readonly Button _entryButton;

        public MainPage()
	    {
	        InitializeComponent();
            _loginLabel = new Label
            {
                Text = "Логин",
                FontSize = 28
            };
            _loginEntry = new Entry { Keyboard = Keyboard.Telephone, Text = "", FontSize = 28 };
            _passwordLabel = new Label
            {
                Text = "Пароль",
                FontSize = 28
            };
            _passwordEntry = new Entry { Text = "", IsPassword = true, FontSize = 28 };
            _entryButton = new Button
            {
                Text = "Войти",
                FontSize = 28
            };
            _entryButton.Clicked += EntryButton_OnClicked;

            var grid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition {Height = 60},
                    new RowDefinition {Height = 60},
                    new RowDefinition {Height = 60}
                },
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };
            grid.Children.Add(_loginLabel, 0, 0);
            grid.Children.Add(_loginEntry, 1, 0);
            grid.Children.Add(_passwordLabel, 0, 1);
            grid.Children.Add(_passwordEntry, 1, 1);
            grid.Children.Add(_entryButton, 1, 2);
            Content = grid;
        }

        private async void EntryButton_OnClicked(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(_loginEntry.Text) || string.IsNullOrWhiteSpace(_passwordEntry.Text))
            {
                await DisplayAlert("Ошибка", "Необходимо ввести корректные логин и пароль", "Закрыть");

                return;
            }
            var info = await new AuthService().GetParticipantInfo(_loginEntry.Text, _passwordEntry.Text);
            if (string.IsNullOrWhiteSpace(info.ContractNumber))
            {
                info.ContractNumber = "123";
            }
            await Navigation.PushAsync(new InfoPage(info));
        }
    }
}
