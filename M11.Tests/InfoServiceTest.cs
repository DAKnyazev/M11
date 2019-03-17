﻿using System;
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
            { "2017.09", 1 }
        };
        private const int Amount = 100;

        private string _phone;
        private string _accountId;

        private readonly InfoService _infoService;
        private readonly CalculatorService _calculatorService; 
        private AccountBalance _accountBalance;

        public InfoServiceTest()
        {
            _infoService = new InfoService();
            _calculatorService = new CalculatorService();
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
                });
                
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

        [Test, Order(4)]
        public async Task TestCalculator()
        {
            Assert.IsTrue(await _calculatorService.TryLoadAsync());
            var result = _calculatorService.Calculate("1", "ponedelnik", "00000100", "moskva", "zelenograd");
            Assert.IsTrue(result.CashCost > 0);
            Assert.IsTrue(result.TransponderCost > 0);
            var departures = _calculatorService.GetDepartures();
            Assert.IsNotNull(departures);
            Assert.IsTrue(departures.Count > 0);
            var destinations = _calculatorService.GetDestinations();
            Assert.IsNotNull(destinations);
            Assert.IsTrue(destinations.Count > 0);
        }

        [Test, Order(5)]
        public async Task TestTicketService()
        {
            var ticketService = new TicketService("https://private.15-58m11.ru/onyma/oss_task_list/form-action/?action_100110000000000000506=1&__form_data__pmodel_id=3101270000000000000033&__form_data__s$pmodel_arg=3101270000000000000033&__parent_obj__=3100520000000000056242&__form_data__group_id=3100150000000000109132&",
                _accountBalance.CookieContainer);
            ticketService.Start();
        }
    }
}