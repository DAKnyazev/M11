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
		public StatisticPage()
		{
			InitializeComponent();
		}

        protected override async void OnAppearing()
        {
            LoadingLabel.IsVisible = true;
            StatisticLayout.Children.Clear();
            await Task.Run(async () => await InitializeAsync());
            //await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (!await App.TryGetInfo())
            {
                await Navigation.PushAsync(new AuthPage());
                return;
            }
            
            var info = await new InfoService().GetAccountInfo(
                App.Info.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                App.Info.CookieContainer,
                DateTime.Now,
                DateTime.Now.AddMonths(-5));
            
            foreach (var item in info.BillSummaryList)
            {
                var layout = new RelativeLayout();
                layout.Children.Add(new Label { Text = item.Period.ToString("MMMM yyyy").FirstCharToUpper() },
                    Constraint.RelativeToParent(parent => 0),
                    null,
                    Constraint.RelativeToParent(parent => parent.Width),
                    Constraint.Constant(36));

                Device.BeginInvokeOnMainThread(() => { StatisticLayout.Children.Add(layout); });
            }

            LoadingLabel.IsVisible = false;
        }
    }
}