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
        private static readonly int Amount = 100;
        private static readonly string Phone = "****";

        private readonly InfoService _infoService;
        private Info _info;

        public InfoServiceTest()
        {
            _infoService = new InfoService();
        }

        [Test, SetUp, Order(1)]
        public async Task TestGetInfo()
        {
            _info = await _infoService.GetInfo(Login, Password);
        }

        [Test, Order(2)]
        public async Task TestGetAccountInfo()
        {
            Assert.IsNotNull(_info);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_info.ContractNumber));
            Assert.IsNotNull(_info.Links);

            var accountInfo = await _infoService.GetAccountInfo(_info.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl,
                _info.CookieContainer, 
                DateTime.Now, 
                DateTime.Now.AddMonths(-5));
            
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.DataObjectId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.IlinkId));
            Assert.IsFalse(string.IsNullOrWhiteSpace(accountInfo.PartyId));
            Assert.IsNotNull(accountInfo.BillSummaryList);
        }

        [Test, Order(3)]
        public async Task TestGetLoginPageContent()
        {
            var content = await _infoService.GetLoginPageContent(Login, Password, typeof(PaymentPage));

            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
        }

        [Test, Order(4)]
        public async Task TestGetPaymentPageContent()
        {
            var content = await _infoService.GetPaymentPageContent(AccountId, Amount, Phone, typeof(PaymentPage));

            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
        }
    }
}