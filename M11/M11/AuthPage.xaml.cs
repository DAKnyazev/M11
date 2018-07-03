using System;
using Xamarin.Forms;

namespace M11
{
	public partial class AuthPage : ContentPage
	{
        public AuthPage()
	    {
	        InitializeComponent();
        }

        private async void EntryButton_OnClicked(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(LoginEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Ошибка", "Необходимо ввести корректные логин и пароль", "Закрыть");

                return;
            }

            if (await App.TryGetInfo(LoginEntry.Text, PasswordEntry.Text))
            {
                await Navigation.PushAsync(new MainPage());
            }
            else
            {
                await DisplayAlert("Ошибка авторизации", "Некорректные логин и/или пароль", "Закрыть");
            }
        }
	}
}
