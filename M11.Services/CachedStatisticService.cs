using System;
using System.Collections.Generic;
using System.Linq;
using M11.Common;
using M11.Common.Models.BillSummary;
using RestSharp;

namespace M11.Services
{
    public class CachedStatisticService
    {
        private readonly StatisticService _statisticService = new StatisticService();
        private readonly GenericDatabase<MonthBillSummary> _repository;

        public CachedStatisticService(GenericDatabase<MonthBillSummary> repository)
        {
            _repository = repository;
        }

        public List<MonthBillSummary> GetMonthlyStatistic(
            IRestClient client,
            string path,
            DateTime start,
            DateTime end,
            string accountId)
        {
            var result = new List<MonthBillSummary>();
            var cachedList = _repository.GetItemsAsync().Result;
            var newStart = new DateTime(start.Year, start.Month, 1);
            while (newStart <= end.Date)
            {
                var cachedItem = cachedList.FirstOrDefault(x => x.IsPeriodEquals(newStart));
                if (cachedItem != null)
                {
                    result.Add(cachedItem);
                    newStart = newStart.AddMonths(1);
                    continue;
                }
                break;
            }

            if (newStart <= end)
            {
                var list = _statisticService.GetMonthlyStatistic(client, path, newStart, end, accountId);

                foreach (var monthBillSummary in list)
                {
                    var count = _repository.SaveItemAsync(monthBillSummary).Result;
                }

                result.AddRange(list);
            }

            return result;
        }

        public List<MonthBillGroup> GetMonthlyDetails(
            string accountPath, 
            IRestClient client, 
            string accountId,
            MonthBillSummary monthlyBillSummary)
        {
            return _statisticService.GetMonthlyDetails(accountPath, client, accountId, monthlyBillSummary);
        }
    }
}
