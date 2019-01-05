using Xamarin.Forms;

namespace M11
{
    /// <summary>
    /// Базовая страница (без навигационного заголовка)
    /// </summary>
    public abstract partial class BaseContentPage : ContentPage
    {
        protected BaseContentPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}
