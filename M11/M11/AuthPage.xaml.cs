using System;
using System.Configuration;
using System.Net;
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

            var status = App.TrySignIn(LoginEntry.Text, PasswordEntry.Text);
            switch (status)
            {
                case HttpStatusCode.OK:
                    App.GoToMainPage(false);
                    break;
                case HttpStatusCode.Unauthorized:
                    await DisplayAlert("Ошибка авторизации", "Некорректные логин и/или пароль", "Закрыть");
                    break;
                default:
                    await DisplayAlert("Ошибка загрузки данных", "Возможно ведутся работы. Попробуйте зайти позднее.", "Закрыть");
                    break;
            }

            AuthActivityIndicator.IsRunning = false;
        }

        private async void OpenCalculator(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new CalculatorPage()));
        }
    }
}
