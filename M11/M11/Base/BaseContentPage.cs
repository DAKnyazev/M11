using Xamarin.Forms;

namespace M11
{
    /// <summary>
    /// Базовая страница (без навигационного заголовка)
    /// </summary>
    public abstract class BaseContentPage : ContentPage
    {
        protected BaseContentPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
