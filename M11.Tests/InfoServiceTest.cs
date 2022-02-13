using System;
using System.Configuration;
using System.Linq;
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
            Assert.IsNotNull(_accountBalance.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl);
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
                new DateTime(2018, 5, 1),
                new DateTime(2018, 5, 1).AddMonths(App.AccountInfoMonthCount + 6));

            _accountId = accountInfo.AccountId;
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.AccountId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.DataObjectId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.IlinkId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.PartyId));
        }

        [Test, Order(2)]
        public void TestGetAccountInfoCached()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var accountInfo = _infoService.GetAccountInfo(
                _accountBalance.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                _accountBalance.CookieContainer,
                new DateTime(2018, 5, 1),
                new DateTime(2018, 5, 1).AddMonths(App.AccountInfoMonthCount + 6));

            _accountId = accountInfo.AccountId;
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.AccountId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.DataObjectId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.IlinkId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.PartyId));
        }

        [Test, Order(3)]
        public void TestGetLoginPageContent()
        {
            var content = _infoService.GetLoginPageContent(Login, Password, typeof(PaymentPage));

            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
        }

        [Test, Order(4)]
        public void TestGetPaymentPageContent()
        {
            var content = _infoService.GetPaymentPageContent(_accountId, Amount, _phone, typeof(PaymentPage));

            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
        }
    }
}