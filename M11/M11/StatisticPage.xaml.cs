using System;
using M11.Base;
using Xamarin.Forms;

namespace M11
{
    public partial class StatisticPage : BaseWebViewPage
    {
        private const string DetailsPageUrl = "https://lk.m11-neva.ru/#/balance/details";

        public StatisticPage() : base(DetailsPageUrl)
		{
			InitializeComponent();
            Setup();
        }

        protected override RelativeLayout GetLayout()
        {
            return StatisticLayout;
        }
    }
}