using System;
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
        private static readonly string Login = "****";
        private static readonly string Password = "****";
        private static readonly string AccountId = "****";
        private static readonly string Phone = "****";
        private static readonly int Amount = 100;

        private readonly InfoService _infoService;
        private Info _info;

        public InfoServiceTest()
        {
            _infoService = new InfoService();
        }

        [Test, SetUp, Order(1)]
        public void TestGetInfo()
        {
            _info = _infoService.GetInfo(Login, Password);
        }

        [Test, Order(2)]
        public void TestGetAccountInfo()
        {
            Assert.IsNotNull(_info);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_info.ContractNumber));
            Assert.IsNotNull(_info.Links);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_info.Phone));

            var accountInfo = _infoService.GetAccountInfo(_info.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                _info.CookieContainer, 
                DateTime.Now, 
                DateTime.Now.AddMonths(-5));

            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.AccountId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.DataObjectId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.IlinkId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.PartyId));
            Assert.IsNotNull(accountInfo.BillSummaryList);
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