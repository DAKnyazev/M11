using System;
using System.Collections.Generic;
using System.Linq;
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
	    private const string LoginKeyName = "Login";
	    private const string PasswordKeyName = "Password";
	    private const string AccountIdKeyName = "AccountId";
	    private const string DataObjectIdKeyName = "DataObjectId";
	    private const string MonthBillSummaryLinkIdKeyName = "MonthBillSummaryLinkId";
        public static int CachingTimeInMinutes { get; set; }
        public static int AccountInfoMonthCount { get; set; }
        public static Credentials Credentials { get; set; }
        public static AccountBalance AccountBalance { get; set; }
        public static AccountInfo AccountInfo { get; set; }
	    public static bool IsNeedReloadMainPage =>
	        (AccountInfo?.RequestDate ?? DateTime.MinValue) < DateTime.Now.AddMinutes(-CachingTimeInMinutes);
        public static string MainColor { get; set; }

        private static object _getAccountInfoLockObject = new object();
        
        static App()
        {
            CachingTimeInMinutes = 3;
            AccountInfoMonthCount = 4;
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
		        Credentials.Login = GetValueFromStorage(LoginKeyName);
		        Credentials.Password = GetValueFromStorage(PasswordKeyName);
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
	        CrossSecureStorage.Current.SetValue(LoginKeyName, string.Empty);
	        CrossSecureStorage.Current.SetValue(PasswordKeyName, string.Empty);
            AccountBalance = new AccountBalance();
            AccountInfo = new AccountInfo();
            Credentials = new Credentials();
	        Current.MainPage = new AuthPage();
        }

        public static bool TryGetInfo()
        {
            return TryGetInfo(Credentials.Login, Credentials.Password);
        }

        public static bool TryGetInfo(string login, string password)
        {
            lock (_getAccountInfoLockObject)
            {
                if (AccountBalance.RequestDate > DateTime.Now.AddMinutes(-CachingTimeInMinutes))
                {
                    return true;
                }

                var accountBalance = new InfoService().GetAccountBalance(login, password);
                if (string.IsNullOrWhiteSpace(accountBalance.ContractNumber))
                {
                    return false;
                }

                AccountBalance = accountBalance;
                CrossSecureStorage.Current.SetValue(LoginKeyName, login);
                CrossSecureStorage.Current.SetValue(PasswordKeyName, password);
                Credentials.Login = login;
                Credentials.Password = password;
            }

            return true;
        }

	    public static void SetUpAccountInfo()
	    {
	        lock (_getAccountInfoLockObject)
	        {
	            if (string.IsNullOrWhiteSpace(AccountInfo?.AccountId) 
	                || AccountInfo.RequestDate < DateTime.Now.AddMinutes(-CachingTimeInMinutes))
	            {
	                try
	                {
	                    AccountInfo = new InfoService().GetAccountInfo(
	                        AccountBalance.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
	                        AccountBalance.CookieContainer,
	                        DateTime.Now,
	                        DateTime.Now.AddMonths(-AccountInfoMonthCount),
	                        GetValueFromStorage(AccountIdKeyName),
	                        GetValueFromStorage(DataObjectIdKeyName));
	                    CrossSecureStorage.Current.SetValue(AccountIdKeyName, AccountInfo.AccountId);
	                    CrossSecureStorage.Current.SetValue(DataObjectIdKeyName, AccountInfo.DataObjectId);
	                }
	                catch (Exception e)
	                {
	                    throw;
	                }
                }
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

	    public static List<Bill> GetLastBills()
	    {
	        var monthBillSummary = AccountInfo.BillSummaryList.OrderByDescending(x => x.Period).FirstOrDefault();
	        if (monthBillSummary == null)
	        {
                return new List<Bill>();
	        }

	        if (monthBillSummary.Groups.Any() &&
	            monthBillSummary.GroupsRequestDate > DateTime.Now.AddMinutes(-CachingTimeInMinutes))
	        {
	            return monthBillSummary.Groups.SelectMany(x => x.Bills).OrderBy(x => x.Period).Take(5).ToList();
            }

	        monthBillSummary.LinkId = GetValueFromStorage($"{MonthBillSummaryLinkIdKeyName}{monthBillSummary.Id}");

	        monthBillSummary.Groups = new InfoService().GetMonthlyDetails(
	            AccountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
	            AccountInfo.RestClient,
	            AccountInfo.IlinkId,
	            AccountInfo.AccountId,
                monthBillSummary);
	        CrossSecureStorage.Current.SetValue($"{MonthBillSummaryLinkIdKeyName}{monthBillSummary.Id}", monthBillSummary.LinkId);

	        return monthBillSummary
	            .Groups
	            .SelectMany(x => x.Bills)
	            .OrderByDescending(x => x.Period)
	            //.Take(5)
	            .ToList();
	    }

        private static string GetValueFromStorage(string key)
	    {
            return CrossSecureStorage.Current.HasKey(key) ? CrossSecureStorage.Current.GetValue(key) : string.Empty;
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
