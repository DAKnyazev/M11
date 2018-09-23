﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using M11.Common.Enums;
using M11.Common.Extentions;
using M11.Services;
using Xamarin.Forms;

namespace M11
{
	public partial class StatisticPage : BaseContentPage
    { 
        private ActivityIndicator LoadingIndicator { get; set; }

        public StatisticPage()
		{
			InitializeComponent();
		    LoadingIndicator = new ActivityIndicator
		    {
                Color = Color.FromHex("#996600")
		    };
        }

        protected override async void OnAppearing()
        {
            LoadingIndicator.IsRunning = true;
            StatisticLayout.Padding = new Thickness(0, 200, 0, 0);
            StatisticLayout.Children.Clear();
            StatisticLayout.Children.Add(LoadingIndicator);
            await Task.Run(async () => await InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            if (!await App.TryGetInfo())
            {
                await Navigation.PushAsync(new AuthPage());
                return;
            }

            if (App.AccountInfo.RequestDate <= DateTime.Now.AddMinutes(-App.CachingTimeInMinutes))
            {
                App.AccountInfo = await new InfoService().GetAccountInfo(
                    App.Info.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                    App.Info.CookieContainer,
                    DateTime.Now,
                    DateTime.Now.AddMonths(-App.AccountInfoMonthCount));
            }

            const int padding = 10;
            foreach (var item in App.AccountInfo.BillSummaryList)
            {
                var layout = new RelativeLayout();
                layout.Children.Add(new BoxView { BackgroundColor = Color.FromHex("#F5F5DC") },
                    Constraint.Constant(padding),
                    Constraint.Constant(0),
                    Constraint.RelativeToParent(parent => parent.Width - 2 * padding),
                    Constraint.Constant(65));
                layout.Children.Add(new Label { Text = item.Period.ToString("MMMM yyyy").FirstCharToUpper(), FontSize = 18 },
                    Constraint.Constant(2 * padding),
                    Constraint.Constant(0),
                    Constraint.RelativeToParent(parent => parent.Width - 4 * padding),
                    Constraint.Constant(36));
                layout.Children.Add(new Label { Text = "+", TextColor = Color.Green, FontSize = 30 },
                    Constraint.RelativeToParent(parent => parent.Width / 2),
                    Constraint.Constant(-3),
                    Constraint.Constant(2 * padding),
                    Constraint.Constant(4 * padding));
                layout.Children.Add(new Label { Text = item.Income.ToString("G", CultureInfo.InvariantCulture), FontSize = 26 },
                    Constraint.RelativeToParent(parent => parent.Width / 2 + 2 * padding),
                    Constraint.Constant(0),
                    Constraint.RelativeToParent(parent => parent.Width / 2 - 4 * padding),
                    Constraint.Constant(4 * padding));
                layout.Children.Add(new Label { Text = "-", TextColor = Color.Red, FontSize = 30 },
                    Constraint.RelativeToParent(parent => parent.Width / 2),
                    Constraint.Constant(3 * padding - 5),
                    Constraint.Constant(2 * padding),
                    Constraint.Constant(4 * padding));
                layout.Children.Add(new Label { Text = item.Spending.ToString("G", CultureInfo.InvariantCulture), FontSize = 26 },
                    Constraint.RelativeToParent(parent => parent.Width / 2 + 2 * padding),
                    Constraint.Constant(3 * padding),
                    Constraint.RelativeToParent(parent => parent.Width / 2 - 4 * padding),
                    Constraint.Constant(4 * padding));

                Device.BeginInvokeOnMainThread(() => { StatisticLayout.Children.Add(layout); });
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsRunning = false;
                StatisticLayout.Padding = new Thickness(0, 0, 0, 0);
            });
        }
    }
}