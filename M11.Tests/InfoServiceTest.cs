using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using M11.Common.Enums;
using M11.Common.Models;
using M11.Services;
using NUnit.Framework;

namespace M11.Tests
{
    [TestFixture]
    public class InfoServiceTest
    {
        private static readonly string Login = ConfigurationManager.AppSettings["login"];
        private static readonly string Password = ConfigurationManager.AppSettings["password"];
        private static readonly Dictionary<string, int> Periods = new Dictionary<string, int>
        {
            { "2018.08", 47 },
            { "2018.07", 21 },
            { "2018.06", 5 },
            { "2019.01", 2 },
            { "2017.09", 1 }
        };
        private const int Amount = 100;

        private string _phone;
        private string _accountId;

        private readonly InfoService _infoService;
        private AccountBalance _accountBalance;

        public InfoServiceTest()
        {
            _infoService = new InfoService();
        }

        [Test, SetUp]
        public void TestGetInfo()
        {
            _accountBalance = _infoService.GetAccountBalance(Login, Password);
            _phone = _accountBalance.Phone;
            Assert.IsNotNull(_accountBalance);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_accountBalance.ContractNumber));
            Assert.IsNotNull(_accountBalance.Links);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_accountBalance.Phone));
        }

        [Test, Order(1)]
        public void TestGetAccountInfo()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var accountInfo = _infoService.GetAccountInfo(
                _accountBalance.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                _accountBalance.CookieContainer, 
                DateTime.Now.AddMonths(-App.AccountInfoMonthCount * 2), 
                DateTime.Now);

            _accountId = accountInfo.AccountId;
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.AccountId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.DataObjectId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.IlinkId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.PartyId));
            Assert.IsNotNull(accountInfo.BillSummaryList);
            try
            {
                Parallel.ForEach(Periods, period =>
                //foreach (var period in Periods)
                {
                    var month = accountInfo.BillSummaryList.First(x => x.PeriodName == period.Key);
                    var groups = _infoService.GetMonthlyDetails(
                        accountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
                        accountInfo.RestClient,
                        accountInfo.IlinkId,
                        accountInfo.AccountId,
                        month);
                    var count = groups.SelectMany(x => x.Bills).Count();
                    Assert.IsTrue(count == period.Value);
                }
                );
                
                watch.Stop();
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [Test, Order(2)]
        public void TestGetLoginPageContent()
        {
            var content = _infoService.GetLoginPageContent(Login, Password, typeof(PaymentPage));

            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
        }

        [Test, Order(3)]
        public void TestGetPaymentPageContent()
        {
            var content = _infoService.GetPaymentPageContent(_accountId, Amount, _phone, typeof(PaymentPage));

            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
        }
    }
}