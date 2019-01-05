using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using M11.Common.Enums;
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
        public static int CachingTimeInMinutes { get; set; }
        public static int AccountInfoMonthCount { get; set; }
        public static int LastBillsMonthCount { get; set; }
        public static Credentials Credentials { get; set; }
        public static AccountBalance AccountBalance { get; set; }
        public static AccountInfo AccountInfo { get; set; }
	    public static bool IsNeedReloadMainPage =>
	        (AccountInfo?.RequestDate ?? DateTime.MinValue) < DateTime.Now.AddMinutes(-CachingTimeInMinutes);
        public static string MainColor { get; set; }

        private static readonly object GetAccountInfoLockObject = new object();
        
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
            lock (GetAccountInfoLockObject)
            {
                if (AccountBalance.RequestDate > DateTime.Now.AddMinutes(-CachingTimeInMinutes))
                {
                    return HttpStatusCode.OK;
                }

                var accountBalance = new InfoService().GetAccountBalance(login, password);
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
	            AccountInfo = new InfoService().GetAccountInfo(
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
	        return new InfoService().GetLoginPageContent(
	            Credentials.Login, 
	            Credentials.Password,
	            type);
	    }

	    public static string GetPaymentPageContent(Type type)
	    {
	        return new InfoService().GetPaymentPageContent(
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

	            monthBillSummary.Groups = new InfoService().GetMonthlyDetails(
	                AccountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
	                AccountInfo.RestClient,
	                AccountInfo.IlinkId,
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

	                monthBillSummary.Groups = new InfoService().GetMonthlyDetails(
	                    AccountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
	                    AccountInfo.RestClient,
	                    AccountInfo.IlinkId,
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

	    public static string GetPointName(string point)
	    {
	        if (point == null)
	        {
	            return string.Empty;
	        }

	        var lowerPoint = point.ToLower();

            if (lowerPoint.Contains("зеленоград"))
	        {
	            return "Зеленоград";
	        }

	        if (lowerPoint.Contains("клин"))
	        {
	            return "Клин";
	        }

	        if (lowerPoint.Contains("москва"))
	        {
	            return "Москва";
	        }

	        if (lowerPoint.Contains("солнечн"))
	        {
	            if (lowerPoint.Contains("67"))
	            {
	                return "Солнечногорск (Пятн. ш.)";
	            }
	            return "Солнечногорск (М-10)";
	        }

	        if (lowerPoint.Contains("ямуга"))
	        {
	            return "Ямуга";
	        }

	        if (lowerPoint.Contains("107"))
	        {
	            return "Бетонка (А-107)";
	        }

	        if (lowerPoint.Contains("шереметьево"))
	        {
	            if (lowerPoint.Contains("1"))
	            {
	                return "Шереметьево-1";
	            }

	            return "Шереметьево-2";
	        }

	        if (lowerPoint.Contains("11") && lowerPoint.Contains("58"))
	        {
	            return "Конец участка";
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
	            var startIndex = pan.IndexOf("на", StringComparison.InvariantCultureIgnoreCase);
	            var endIndex = pan.IndexOf(",", startIndex, StringComparison.InvariantCultureIgnoreCase);
	            var result = $"Абонемент {pan.Substring(startIndex, endIndex - startIndex)}";
	            startIndex = pan.IndexOf(",", endIndex + 1, StringComparison.InvariantCultureIgnoreCase);
	            endIndex = pan.IndexOf("(", startIndex, StringComparison.InvariantCultureIgnoreCase);

	            return $"{result}\r\n({pan.Substring(startIndex + 2, endIndex - startIndex - 3)})";
            }
	        catch
	        {
	            return string.Empty;
	        }
	    }
	}
}
