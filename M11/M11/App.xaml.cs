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
using M11.Resources;
using M11.Services;
using Plugin.SecureStorage;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Skip)]
namespace M11
{
	public partial class App : Application
	{
        public static GenericDatabase MonthBillSummaryDatabase = 
            GenericDatabase.GetDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonthBillSummarySQLite.db3"));

        public static int CachingTimeInMinutes { get; set; }
        public static int AccountInfoMonthCount { get; set; }
        public static int LastBillsMonthCount { get; set; }
        public static Credentials Credentials { get; set; }
        public static AccountBalance AccountBalance { get; set; }
        public static AccountInfo AccountInfo { get; set; }
        public static decimal AutodorCalculatorPrice { get; set; }

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
		private static readonly TokenService TokenService = new TokenService();

        static App()
        {
            CachingTimeInMinutes = 3;
            AccountInfoMonthCount = 12;
            LastBillsMonthCount = 2;
            Credentials = new Credentials();
	        AccountBalance = new AccountBalance();
            AccountInfo = new AccountInfo();
            MainColor = "#996600";
            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;            
        }

        public App()
		{
			InitializeComponent();
            Resources["GlobalResources"] = GlobalResources.Current;
        }

	    private void Init()
	    {
	        try
	        {
	            Credentials.Login = GetValueFromStorage(CrossSecureStorageKeys.Login);
	            Credentials.Password = GetValueFromStorage(CrossSecureStorageKeys.Password);
	            SetNotificationFrequency();
                SetSettingsBadge();
            }
	        catch
	        {
	            Credentials.Login = string.Empty;
	            Credentials.Password = string.Empty;
	        }

	        try
	        {
	            if (TryGetInfo() != HttpStatusCode.OK)
	            {
	                MainPage = new NavigationPage(new AuthPage());
                    return;
	            }

	            GoToMainPage(false);
	        }
	        catch (Exception e)
	        {
	            MainPage = new NavigationPage(new AuthPage());
            }
        }

	    public static void GoToMainPage(bool isNeedOpenMainTab)
	    {
            if (TryGetInfo() != HttpStatusCode.OK)
            {
                Current.MainPage = new NavigationPage(new AuthPage());
                return;
            }
            SetUpAccountInfo();
	        if (Current.MainPage is TabbedMainPage tabbedMainPage)
	        {
	            if (isNeedOpenMainTab)
	            {
	                tabbedMainPage.CurrentPage = tabbedMainPage.Children[0];
	            }
	            return;
	        }
	        Current.MainPage = new TabbedMainPage();
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
	        ClearDatabase();
	    }

        private static HttpStatusCode TryGetInfo()
        {
            return TryGetInfo(Credentials.Login, Credentials.Password, false);
        }

	    public static HttpStatusCode TrySignIn(string login, string password)
	    {
	        return TryGetInfo(login, password, true);
	    }

        private static HttpStatusCode TryGetInfo(string login, string password, bool isLogin)
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

                var accountBalance = new InfoService(MonthBillSummaryDatabase).GetAccountBalance(login, password);
                if (accountBalance.StatusCode != HttpStatusCode.OK)
                {
                    return accountBalance.StatusCode;
                }
                if (string.IsNullOrWhiteSpace(accountBalance.ContractNumber))
                {
                    return HttpStatusCode.Unauthorized;
                }

                AccountBalance = accountBalance;
                if (isLogin)
                {
                    ClearDatabase();
                    CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.Login, login);
                    CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.Password, password);
                    Credentials.Login = login;
                    Credentials.Password = password;
                }

				Credentials.Token = GetToken(login, password);
			}

            return HttpStatusCode.OK;
        }

	    private static void SetUpAccountInfo()
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
	            AccountBalance.Phone ?? string.Empty,
	            type);
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

        public static void ClearSettingsBadge()
        {
            if (Current.MainPage is TabbedMainPage tabbedMainPage
                && tabbedMainPage.CurrentPage is NavigationPage navigationPage
                && navigationPage.CurrentPage is SettingsPage)
            {
                CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.SettingsBadge, "True");
                GlobalResources.Current.SettingsBadge = string.Empty;
            }
        }

        protected override void OnStart()
	    {
            // Handle when your app starts
            Init();
	    }

	    protected override void OnSleep()
	    {
	        // Handle when your app sleeps
	    }

	    protected override void OnResume()
	    {
            // Handle when your app resumes
	        Init();
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

	    private static void SetNotificationFrequency()
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

        private static void SetSettingsBadge()
        {
            var value = GetValueFromStorage(CrossSecureStorageKeys.SettingsBadge);
            if (string.IsNullOrWhiteSpace(value))
            {
                GlobalResources.Current.SettingsBadge = "1";
                return;
            }
            GlobalResources.Current.SettingsBadge = string.Empty;
        }

	    private static void SaveNotificationFrequency(NotificationFrequency notificationFrequency)
	    {
	        CrossSecureStorage.Current.SetValue(CrossSecureStorageKeys.NotificationFrequency, notificationFrequency.ToString());
        }

	    private static void ClearDatabase()
	    {
	        lock (GetAccountInfoLockObject)
	        {
	            AsyncHelpers.RunSync(() => MonthBillSummaryDatabase.ClearTablesAsync());
	        }
	    }

		private static string GetToken(string login, string password)
        {
			var token = AsyncHelpers.RunSync(() => TokenService.GetTokenAsync(login, password));

			return token;
		}
    }
}
