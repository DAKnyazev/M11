using System.Linq;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using M11.Common.Models;

namespace M11.Droid
{
    [BroadcastReceiver]
    public class BalanceBroadcastReceiver : BroadcastReceiver
    {
        private static AccountBalance _accountBalance;

        public override void OnReceive(Context context, Intent intent)
        {
            var accountBalance = App.GetAccountBalanceForNotification();
            if (accountBalance == null)
            {
                return;
            }

            if (_accountBalance == null)
            {
                _accountBalance = accountBalance;
                return;
            }

            var text = string.Empty;
            var title = string.Empty;

            if (_accountBalance.Balance != accountBalance.Balance)
            {
                decimal.TryParse(_accountBalance.Balance, out var oldBalance);
                decimal.TryParse(accountBalance.Balance, out var currentBalance);
                var diff = currentBalance - oldBalance;
                text = $"{(diff > 0 ? "Пополнение на" : "Списание")} {diff} рублей.";
                title = "М11 - Изменение баланса";
            }
            else
            {
                if (_accountBalance.Tickets != null)
                {
                    foreach (var ticket in _accountBalance.Tickets)
                    {
                        var currentTicket = accountBalance.Tickets?.FirstOrDefault(x => x.StartDate == ticket.StartDate);
                        if (currentTicket == null)
                        {
                            text = $"Закончились поездки по абонементу {ticket.ShortDescription}.";
                            title = "М11 - Абонемент";
                            break;
                        }

                        if (ticket.RemainingTripsCount != currentTicket.RemainingTripsCount)
                        {
                            text = $"Произошло списание поездки по абонементу {ticket.ShortDescription}, остаток {currentTicket.RemainingTripsCount} (из {ticket.TotalTripsCount}) поездок.";
                            title = "М11 - Абонемент";
                            break;
                        }
                    }
                }
            }

            _accountBalance = accountBalance;

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var launchIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
            intent.AddFlags(ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(context, 0, launchIntent, PendingIntentFlags.CancelCurrent);

            var builder = new NotificationCompat.Builder(context, MainActivity.ChannelId)
                .SetContentTitle(title)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(text))
                .SetSmallIcon(Resource.Drawable.notification_icon_background)
                .SetColor(Resource.Color.colorPrimary)
                .SetContentIntent(pendingIntent);

            // Build the notification:
            var notification = builder.Build();

            // Get the notification manager:
            var notificationManager =
                context.GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager?.Notify(notificationId, notification);
        }
    }
}