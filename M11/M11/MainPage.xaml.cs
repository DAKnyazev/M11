using M11.Base;
using Xamarin.Forms;

namespace M11
{
    public partial class MainPage : BaseWebViewPage
    {
        private const string MainPageUrl = "https://lk.m11-neva.ru/";

        public MainPage() : base(MainPageUrl)
        {
            InitializeComponent();
            Setup();
        }

        protected override RelativeLayout GetLayout()
        {
            return MainPageLayout;
        }
    }
}