﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using M11.Common;
using M11.Common.Enums;
using M11.Common.Models;
using M11.Services;
using NUnit.Framework;

namespace M11.Tests
{
    [TestFixture]
    public class InfoServiceTest
    {
        private static readonly GenericDatabase MonthBillSummaryDatabase =
            GenericDatabase.GetDatabase("D:\\MonthBillSummarySQL2.db3");
        private static readonly string Login = ConfigurationManager.AppSettings["login"];
        private static readonly string Password = ConfigurationManager.AppSettings["password"];
        private static readonly Dictionary<string, int> Periods = new Dictionary<string, int>
        {
            { "2018.08", 49 }, // Многостраничные траты
            { "2018.07", 23 },
            { "2018.06", 5 },
            { "2019.05", 16 },
            { "2019.01", 27 },
            { "2019.03", 35 }
        };
        private const int Amount = 100;

        private string _phone;
        private string _accountId;

        private readonly InfoService _infoService;
        private readonly CalculatorService _calculatorService;
        private readonly CachedStatisticService _cachedStatisticService;
        private readonly StatisticService _statisticService;
        private AccountBalance _accountBalance;

        public InfoServiceTest()
        {
            _infoService = new InfoService(MonthBillSummaryDatabase);
            _calculatorService = new CalculatorService();
            _cachedStatisticService = new CachedStatisticService(MonthBillSummaryDatabase);
            _statisticService = new StatisticService();
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
            Assert.IsNotNull(accountInfo.BillSummaryList);
            try
            {
                Parallel.ForEach(Periods, period =>
                {
                    var month = accountInfo.BillSummaryList.First(x => x.PeriodName == period.Key);
                    var groups = _statisticService.GetMonthlyDetails(
                        accountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
                        accountInfo.RestClient,
                        accountInfo.AccountId,
                        month);
                    Assert.IsFalse(groups.IsError);
                    var count = groups.List.SelectMany(x => x.Bills).Count();
                    Assert.AreEqual(period.Value, count);
                });

                watch.Stop();
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
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
            Assert.IsNotNull(accountInfo.BillSummaryList);
            try
            {
                Parallel.ForEach(Periods, period =>
                {
                    var month = accountInfo.BillSummaryList.First(x => x.PeriodName == period.Key);
                    var groups = _cachedStatisticService.GetMonthlyDetails(
                        accountInfo.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
                        accountInfo.RestClient,
                        accountInfo.AccountId,
                        month);
                    var count = groups.SelectMany(x => x.Bills).Count();
                    Assert.AreEqual(period.Value, count);
                });
                
                watch.Stop();
            }
            catch (Exception e)
            {
                Assert.Fail();
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
            var content = _infoService.GetPaymentPageContent(_accountId, Amount, _phone, typeof(PaymentPage));

            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
        }

        [Test, Order(5)]
        public async Task TestCalculator()
        {
            Assert.IsTrue(await _calculatorService.TryLoadAsync());
            var result = _calculatorService.Calculate("1", "ponedelnik", "00000100", "moskva", "zelenograd");
            Assert.IsTrue(result.CashCost > 0);
            Assert.IsTrue(result.TransponderCost > 0);
            var compositeResult = _calculatorService.Calculate("1", "ponedelnik", "00000100", "moskva", "sanktpeterburg684ykm");
            Assert.IsTrue(compositeResult.CashCost > 0);
            Assert.IsTrue(compositeResult.TransponderCost > 0);
            var compositeBackResult = _calculatorService.Calculate("1", "ponedelnik", "00000100", "sanktpeterburg684ykm", "moskva");
            Assert.IsTrue(compositeBackResult.CashCost > 0);
            Assert.IsTrue(compositeBackResult.TransponderCost > 0);
            var departures = _calculatorService.GetDepartures();
            Assert.IsNotNull(departures);
            Assert.IsTrue(departures.Count > 0);
            var destinations = _calculatorService.GetDestinations();
            Assert.IsNotNull(destinations);
            Assert.IsTrue(destinations.Count > 0);
        }
    }
}