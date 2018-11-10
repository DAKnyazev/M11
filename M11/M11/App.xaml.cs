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
        public static int CachingTimeInMinutes { get; set; }
        public static int AccountInfoMonthCount { get; set; }
        public static Credentials Credentials { get; set; }
        public static AccountBalance AccountBalance { get; set; }
        public static AccountInfo AccountInfo { get; set; }
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

            return true;
        }

	    public static void SetUpAccountInfo()
	    {
	        lock (_getAccountInfoLockObject)
	        {
	            if (string.IsNullOrWhiteSpace(AccountInfo?.AccountId) || AccountInfo.RequestDate < DateTime.Now.AddMinutes(-CachingTimeInMinutes))
	            {
	                AccountInfo = new InfoService().GetAccountInfo(
	                    AccountBalance.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
	                    AccountBalance.CookieContainer,
	                    DateTime.Now,
	                    DateTime.Now.AddMonths(-AccountInfoMonthCount));
	                CrossSecureStorage.Current.SetValue(AccountIdKeyName, AccountInfo.AccountId);
	                CrossSecureStorage.Current.SetValue(DataObjectIdKeyName, AccountInfo.DataObjectId);
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

	        monthBillSummary.Groups = new InfoService().GetMonthlyDetails(
	            AccountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
	            AccountInfo.RestClient,
	            AccountInfo.IlinkId,
	            AccountInfo.AccountId,
                monthBillSummary);

	        return monthBillSummary.Groups.SelectMany(x => x.Bills).OrderBy(x => x.Period).Take(5).ToList();
	    }

        private string GetValueFromStorage(string key)
	    {
            return CrossSecureStorage.Current.HasKey(key) ? CrossSecureStorage.Current.GetValue(key) : string.Empty;
        }
	}
}
