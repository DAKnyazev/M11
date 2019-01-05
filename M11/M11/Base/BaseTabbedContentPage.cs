using Xamarin.Forms;

namespace M11
{
    /// <summary>
    /// Базовая страница (без навигационного заголовка)
    /// </summary>
    public class BaseTabbedContentPage : TabbedPage
    {
        public BaseTabbedContentPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
