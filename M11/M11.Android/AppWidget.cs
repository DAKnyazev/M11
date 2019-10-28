using System;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using M11.Common.Models;

namespace M11.Droid
{
    [BroadcastReceiver(Label = "M11 Widget")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
    public class AppWidget : AppWidgetProvider
    {
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
            appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, App.GetAccountBalance()));
        }

        private RemoteViews BuildRemoteViews(Context context, AccountBalance accountBalance)
        {
            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.widget);
            if (accountBalance == null)
            {
                widgetView.SetTextViewText(Resource.Id.widgetMedium, "Баланс недоступен");
                widgetView.SetTextViewText(Resource.Id.widgetSmall, string.Empty);

                return widgetView;
            }

            widgetView.SetTextViewText(Resource.Id.widgetMedium, $"Баланс: {accountBalance.Balance} Р");
            widgetView.SetTextViewText(Resource.Id.widgetSmall, $"Обновлено: {GetUpdatedText(accountBalance)}");

            return widgetView;
        }

        private string GetUpdatedText(AccountBalance accountBalance)
        {
            return accountBalance.RequestDate.Date == DateTime.Now.Date
                ? accountBalance.RequestDate.ToString("HH:mm")
                : accountBalance.RequestDate.ToString("dd.MM.yyyy");
        }
    }
}