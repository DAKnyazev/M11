using System;
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
        private static readonly string Login = "****";
        private static readonly string Password = "****";
        private static readonly string AccountId = "****";
        private static readonly string Phone = "****";
        private static readonly int Amount = 100;

        private readonly InfoService _infoService;
        private AccountBalance _accountBalance;

        public InfoServiceTest()
        {
            _infoService = new InfoService();
        }

        [Test, SetUp, Order(1)]
        public void TestGetInfo()
        {
            _accountBalance = _infoService.GetAccountBalance(Login, Password);
        }

        [Test, Order(2)]
        public void TestGetAccountInfo()
        {
            Assert.IsNotNull(_accountBalance);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_accountBalance.ContractNumber));
            Assert.IsNotNull(_accountBalance.Links);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_accountBalance.Phone));

            var accountInfo = _infoService.GetAccountInfo(_accountBalance.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                _accountBalance.CookieContainer, 
                DateTime.Now, 
                DateTime.Now.AddMonths(-5));

            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.AccountId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.DataObjectId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.IlinkId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.PartyId));
            Assert.IsNotNull(accountInfo.BillSummaryList);
            try
            {
                var groups = _infoService.GetMonthlyDetails(
                    accountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
                    accountInfo.RestClient,
                    accountInfo.IlinkId,
                    accountInfo.AccountId,
                    accountInfo.BillSummaryList.OrderByDescending(x => x.Period).FirstOrDefault());
            }
            catch (Exception e)
            {
            }
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
            var content = _infoService.GetPaymentPageContent(AccountId, Amount, Phone, typeof(PaymentPage));

            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
        }
    }
}