using System;
using Xamarin.Forms;

namespace M11
{
	public partial class AuthPage : BaseContentPage
    {
        public AuthPage()
	    {
	        InitializeComponent();
        }

        private async void EntryButton_OnClicked(object sender, EventArgs e)
        {
            AuthActivityIndicator.IsRunning = true;
            if (string.IsNullOrWhiteSpace(LoginEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Ошибка", "Необходимо ввести корректные логин и пароль", "Закрыть");
                AuthActivityIndicator.IsRunning = false;

                return;
            }
            
            if (App.TryGetInfo(LoginEntry.Text, PasswordEntry.Text))
            {
                Application.Current.MainPage = new TabbedMainPage();
            }
            else
            {
                await DisplayAlert("Ошибка авторизации", "Некорректные логин и/или пароль", "Закрыть");
            }
            AuthActivityIndicator.IsRunning = false;
        }
	}
}
