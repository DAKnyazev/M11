using System;
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
                    DateTime.Now.AddMonths(-5));
            }
            
            foreach (var item in App.AccountInfo.BillSummaryList)
            {
                var layout = new RelativeLayout();
                layout.Children.Add(new Label { Text = item.Period.ToString("MMMM yyyy").FirstCharToUpper() },
                    Constraint.RelativeToParent(parent => 0),
                    null,
                    Constraint.RelativeToParent(parent => parent.Width),
                    Constraint.Constant(36));

                Device.BeginInvokeOnMainThread(() => { StatisticLayout.Children.Add(layout); });
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsRunning = false;
                StatisticLayout.Padding = new Thickness(0, 30, 0, 0);
            });
        }
    }
}