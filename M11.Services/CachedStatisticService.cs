using System;
using System.Collections.Generic;
using System.Linq;
using M11.Common;
using M11.Common.Extentions;
using M11.Common.Models.BillSummary;
using RestSharp;

namespace M11.Services
{
    public class CachedStatisticService
    {
        private readonly StatisticService _statisticService = new StatisticService();
        private readonly GenericDatabase _repository;

        public CachedStatisticService(GenericDatabase repository)
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
            var cachedList = AsyncHelpers.RunSync(() => _repository.GetItemsAsync<MonthBillSummary>());
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
                var listResult = _statisticService.GetMonthlyStatistic(client, path, newStart, end, accountId);

                if (!listResult.IsError)
                {
                    foreach (var monthBillSummary in listResult.List)
                    {
                        AsyncHelpers.RunSync(() => _repository.SaveItemAsync(monthBillSummary));
                    }
                }

                result.AddRange(listResult.List);
            }

            return result;
        }

        public List<MonthBillGroup> GetMonthlyDetails(
            string accountPath, 
            IRestClient client, 
            string accountId,
            MonthBillSummary monthlyBillSummary)
        {
            List<MonthBillGroup> result = null;
            if (!monthlyBillSummary.IsPeriodEquals(DateTime.Now))
            {
                result = AsyncHelpers.RunSync(() =>_repository.GetItemsAsync<MonthBillGroup>())
                    .Where(x => x.MonthBillSummaryId == monthlyBillSummary.Id)
                    .ToList();
                foreach (var group in result)
                {
                    var bills = AsyncHelpers.RunSync(() => _repository.GetItemsAsync<Bill>())
                        .Where(x => x.MonthBillGroupId == group.Id)
                        .ToList();
                    group.Bills = bills;
                }
            }

            if (result?.Any() == true)
            {
                return result;
            }

            
            var detailsResult = _statisticService.GetMonthlyDetails(accountPath, client, accountId, monthlyBillSummary);
            result = detailsResult.List;
            if (!detailsResult.IsError)
            {
                foreach (var group in result)
                {
                    foreach (var bill in group.Bills)
                    {
                        bill.MonthBillGroupId = group.Id;
                        AsyncHelpers.RunSync(() => _repository.SaveItemAsync(bill));
                    }

                    group.MonthBillSummaryId = monthlyBillSummary.Id;
                    AsyncHelpers.RunSync(() => _repository.SaveItemAsync(group));
                }
            }

            return result;
        }
    }
}
