using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using M11.Common;
using M11.Common.Enums;
using M11.Common.Extentions;
using M11.Common.Models;
using M11.Common.Models.BillSummary;
using M11.Services;
using Plugin.SecureStorage;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Skip)]
namespace M11
{
	public partial class App : Application
	{
        public static GenericDatabase<MonthBillSummary> MonthBillSummaryDatabase = 
            new GenericDatabase<MonthBillSummary>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonthBillSummarySQLite.db3"));

        public static int CachingTimeInMinutes { get; set; }
        public static int AccountInfoMonthCount { get; set; }
        public static int LastBillsMonthCount { get; set; }
        public static Credentials Credentials { get; set; }
        public static AccountBalance AccountBalance { get; set; }
        public static AccountInfo AccountInfo { get; set; }
	    public static bool IsNeedReloadMainPage =>
	        (AccountInfo?.RequestDate ?? DateTime.MinValue) < DateTime.Now.AddMinutes(-CachingTimeInMinutes)
	        || string.IsNullOrWhiteSpace(AccountBalance.Balance);
        public static string MainColor { get; set; }

	    public static NotificationFrequency NotificationFrequency
	    {
	        get => _notificationFrequency;
	        set
	        {
	            _notificationFrequency = value;
	            SaveNotificationFrequency(value);
	        }
	    }

	    public static List<NotificationFrequency> NotificationFrequencies =
	        Enum.GetValues(typeof(NotificationFrequency)).Cast<NotificationFrequency>().ToList();

	    public static List<string> NotificationFrequenciesDescriptions =
	        NotificationFrequencies.Select(x => x.GetDescription()).ToList();
	    public static List<string> NotificationFrequenciesFullDescriptions =
	        NotificationFrequencies.Select(x => x.GetFullDescription()).ToList();

        public static int NotificationCount;
	    public static readonly int NotificationCheckIntervalInMinutes = 6;

        private static readonly object GetAccountInfoLockObject = new object();
	    private static NotificationFrequency _notificationFrequency;
	    private static readonly InfoService InfoService = new InfoService(MonthBillSummaryDatabase);
        private static readonly CachedStatisticService CachedStatisticService = new CachedStatisticService(MonthBillSummaryDatabase);

        static App()
        {
            CachingTimeInMinutes = 3;
            AccountInfoMonthCount = 12;
            LastBillsMonthCount = 2;
            Credentials = new Credentials();
	        AccountBalance = new AccountBalance();
            AccountInfo = new AccountInfo();
            MainColor = "#996600";
        }

        public App()
		{
			InitializeComponent();
            try
		    {
		        Credentials.Login = GetValueFromStorage(CrossSecureStorageKeys.Login);
		        Credentials.Password = GetValueFromStorage(CrossSecureStorageKeys.Password);
		        SetUpNotificationFrequency();
		    }
		    catch
		    {
		        Credentials.Login = string.Empty;
		        Credentials.Password = string.Empty;
		    }
		    
		    if (!string.IsNullOrWhiteSpace(Credentials.Login) 
		        && !string.IsNullOrWhiteSpace(Credentials.Password))
		    {
		        MainPage = new NavigationPage(new TabbedMainPage());
            }
		    else
		    {
		        MainPage = new NavigationPage(new AuthPage());
            }
		}

	    public static void Exit()
	    {
	        CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.Login, string.Empty);
	        CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.Password, string.Empty);
	        CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.AccountId, string.Empty);
	        CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.DataObjectId, string.Empty);
            AccountBalance = new AccountBalance();
            AccountInfo = new AccountInfo();
            Credentials = new Credentials();
	        Current.MainPage = new AuthPage();
        }

        public static HttpStatusCode TryGetInfo()
        {
            return TryGetInfo(Credentials.Login, Credentials.Password);
        }

        public static HttpStatusCode TryGetInfo(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                return HttpStatusCode.Unauthorized;
            }

            lock (GetAccountInfoLockObject)
            {
                if (AccountBalance.RequestDate > DateTime.Now.AddMinutes(-CachingTimeInMinutes) && !string.IsNullOrWhiteSpace(AccountBalance.Balance))
                {
                    return HttpStatusCode.OK;
                }

                var accountBalance = new InfoService(App.MonthBillSummaryDatabase).GetAccountBalance(login, password);
                if (accountBalance.StatusCode != HttpStatusCode.OK)
                {
                    return accountBalance.StatusCode;
                }
                if (string.IsNullOrWhiteSpace(accountBalance.ContractNumber))
                {
                    return HttpStatusCode.Unauthorized;
                }

                AccountBalance = accountBalance;
                CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.Login, login);
                CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.Password, password);
                Credentials.Login = login;
                Credentials.Password = password;
            }

            return HttpStatusCode.OK;
        }

	    public static void SetUpAccountInfo()
	    {
	        lock (GetAccountInfoLockObject)
	        {
	            if (!string.IsNullOrWhiteSpace(AccountInfo?.AccountId) &&
	                AccountInfo.RequestDate >= DateTime.Now.AddMinutes(-CachingTimeInMinutes))
	            {
	                return;
	            }
	            AccountInfo = new InfoService(App.MonthBillSummaryDatabase).GetAccountInfo(
	                AccountBalance.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
	                AccountBalance.CookieContainer,
	                DateTime.Now.AddMonths(-AccountInfoMonthCount),
	                DateTime.Now,
	                GetValueFromStorage(CrossSecureStorageKeys.AccountId),
	                GetValueFromStorage(CrossSecureStorageKeys.DataObjectId));
	            CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.AccountId, AccountInfo.AccountId);
	            CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.DataObjectId, AccountInfo.DataObjectId);
	        }
        }

	    public static string GetLoginPageContent(Type type)
	    {
	        return InfoService.GetLoginPageContent(
	            Credentials.Login, 
	            Credentials.Password,
	            type);
	    }

	    public static string GetPaymentPageContent(Type type)
	    {
	        return InfoService.GetPaymentPageContent(
	            AccountInfo.AccountId, 
	            100, 
	            AccountBalance.Phone,
	            type);
	    }

	    public static void FillGroups(MonthBillSummary monthBillSummary)
	    {
	        try
	        {
	            if (monthBillSummary.Groups.Any()
	                && monthBillSummary.GroupsRequestDate > DateTime.Now.AddMinutes(-CachingTimeInMinutes))
	            {
	                return;
	            }

	            monthBillSummary.Groups = CachedStatisticService.GetMonthlyDetails(
	                AccountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
	                AccountInfo.RestClient,
	                AccountInfo.AccountId,
	                monthBillSummary);
                monthBillSummary.GroupsRequestDate = DateTime.Now;
            }
	        catch (Exception e)
	        {
	            return;
	        }
        }

	    public static List<Bill> GetLastBills()
	    {
	        var months = AccountInfo.BillSummaryList.OrderByDescending(x => x.Period).Take(LastBillsMonthCount).ToList();
	        if (!months.Any())
	        {
                return new List<Bill>();
	        }

	        Parallel.ForEach(months, monthBillSummary =>
	            {
	                if (monthBillSummary.Groups.Any() &&
	                    monthBillSummary.GroupsRequestDate > DateTime.Now.AddMinutes(-CachingTimeInMinutes))
	                {
	                    return;
	                }

	                monthBillSummary.Groups = CachedStatisticService.GetMonthlyDetails(
	                    AccountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
	                    AccountInfo.RestClient,
	                    AccountInfo.AccountId,
	                    monthBillSummary);
	            }
	        );

	        return months
                .SelectMany(x => x.Groups.SelectMany(y => y.Bills))
                .Distinct()
	            .OrderByDescending(x => x.Period)
                .ToList();
	    }

	    public static string GetPointName(string point)
	    {
	        if (point == null)
	        {
	            return string.Empty;
	        }

	        var lowerPoint = point.ToLower();

            if (lowerPoint.Contains("����������"))
	        {
	            return "����������";
	        }

	        if (lowerPoint.Contains("����"))
	        {
	            return "����";
	        }

	        if (lowerPoint.Contains("������"))
	        {
	            return "������";
	        }

	        if (lowerPoint.Contains("�������"))
	        {
	            if (lowerPoint.Contains("67"))
	            {
	                return "������������� (����. �.)";
	            }
	            return "������������� (�-10)";
	        }

	        if (lowerPoint.Contains("�����"))
	        {
	            return "�����";
	        }

	        if (lowerPoint.Contains("107"))
	        {
	            return "������� (�-107)";
	        }

	        if (lowerPoint.Contains("�����������"))
	        {
	            if (lowerPoint.Contains("1"))
	            {
	                return "�����������-1";
	            }

	            return "�����������-2";
	        }

	        if (lowerPoint.Contains("11") && lowerPoint.Contains("58"))
	        {
	            return "����� �������";
	        }
                 
	        return point;
	    }

	    public static string GetTicketDescription(string pan)
	    {
	        if (string.IsNullOrWhiteSpace(pan))
	        {
	            return string.Empty;
	        }

	        try
	        {
	            var startIndex = pan.IndexOf("��", StringComparison.InvariantCultureIgnoreCase);
	            var endIndex = pan.IndexOf(",", startIndex, StringComparison.InvariantCultureIgnoreCase);
	            var result = $"��������� {pan.Substring(startIndex, endIndex - startIndex)}";
	            startIndex = pan.IndexOf(",", endIndex + 1, StringComparison.InvariantCultureIgnoreCase);
	            endIndex = pan.IndexOf("(", startIndex, StringComparison.InvariantCultureIgnoreCase);

	            return $"{result}\r\n({pan.Substring(startIndex + 2, endIndex - startIndex - 3)})";
            }
	        catch
	        {
	            return string.Empty;
	        }
	    }

	    public static AccountBalance GetAccountBalance()
	    {
	        if (TryGetInfo() == HttpStatusCode.OK)
	        {
	            return AccountBalance;
	        }

            return null;
	    }

	    public static AccountBalance GetAccountBalanceForNotification()
	    {
	        AccountBalance.RequestDate = DateTime.MinValue;

            if (NotificationFrequency != NotificationFrequency.Off)
	        {
	            NotificationCount++;
	            if (NotificationCount == (int) NotificationFrequency)
	            {
	                NotificationCount = 0;
                    return GetAccountBalance();
                }
            }

	        return null;
	    }

	    public static void SetNeedReload()
	    {
	        if (AccountInfo != null)
	        {
	            AccountInfo.RequestDate = DateTime.MinValue;
	        }
	    }

        protected override void OnStart()
	    {
	        // Handle when your app starts
	    }

	    protected override void OnSleep()
	    {
	        // Handle when your app sleeps
	    }

	    protected override void OnResume()
	    {
	        // Handle when your app resumes
	    }

        private static string GetValueFromStorage(string key)
	    {
	        try
	        {
	            return CrossSecureStorage.Current.HasKey(key) ? CrossSecureStorage.Current.GetValue(key) : string.Empty;
	        }
	        catch
	        {
	            return string.Empty;
	        }
	    }

	    private static void SetUpNotificationFrequency()
	    {
	        var value = GetValueFromStorage(CrossSecureStorageKeys.NotificationFrequency);
	        if (!string.IsNullOrWhiteSpace(value)
	            && Enum.TryParse(value, out NotificationFrequency frequency))
	        {
	            NotificationFrequency = frequency;
	        }
	        else
	        {
	            NotificationFrequency = NotificationFrequency.Off;
	        }
	    }

	    private static void SaveNotificationFrequency(NotificationFrequency notificationFrequency)
	    {
	        CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.NotificationFrequency, notificationFrequency.ToString());
        }
    }
}
