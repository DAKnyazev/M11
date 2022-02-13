using Xamarin.Forms;
using M11.Base;
using M11.Droid;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Android.Views;

[assembly: ExportRenderer(typeof(ScrollableWebView), typeof(ScrollableWebViewRenderer))]
namespace M11.Droid
{
    internal class ScrollableWebViewRenderer : WebViewRenderer
    {
        public ScrollableWebViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            Parent.RequestDisallowInterceptTouchEvent(true);
            return base.DispatchTouchEvent(e);
        }
    }
}