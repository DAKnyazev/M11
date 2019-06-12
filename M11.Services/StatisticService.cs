using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using M11.Common.Models.BillSummary;
using Newtonsoft.Json;
using RestSharp;

namespace M11.Services
{
    public class StatisticService : BaseInfoService
    {
        private static readonly object FillBillsLockObject = new object();
        private static readonly string MonthlyBillsPath = "month_bills2/";

        public List<MonthBillSummary> GetMonthlyStatistic(
            IRestClient client, 
            string path, 
            DateTime start,
            DateTime end, 
            string accountId)
        {
            var result = new List<MonthBillSummary>();
            var filterUrl = SaveMonthlyBillsFilter + path.Substring(path.IndexOf('?')) + $"&__parent_obj__={accountId}";
            var param = JsonConvert.SerializeObject(new List<BillsSummaryParam> { new BillsSummaryParam(start, end) });
            var filterRequest = new RestRequest(filterUrl, Method.POST);
            filterRequest.AddParameter("raw_state", param);
            var request = new RestRequest(path + "&simple=1", Method.POST);
            request.AddParameter("raw_state", param);
            client.Execute(filterRequest);
            var response = client.Execute(request);
            var content = Regex.Replace(response.Content, @"\\t|\\n|\\r|\\", "");
            var tbody = GetTagValue(content, "<tbody>", "</tbody>");
            var document = new HtmlDocument();
            document.LoadHtml(tbody);
            try
            {
                var i = 0;
                while (true)
                {
                    i++;
                    if (string.IsNullOrWhiteSpace(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[1]//text()")?.InnerText))
                    {
                        break;
                    }

                    decimal.TryParse(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[2]//text()").InnerText.Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var income);
                    decimal.TryParse(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[3]//text()").InnerText.Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var spending);
                    decimal.TryParse(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[4]//text()").InnerText.Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var startBalance);
                    decimal.TryParse(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[5]//text()").InnerText.Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var endBalance);
                    result.Add(new MonthBillSummary
                    {
                        Id = document.DocumentNode.SelectSingleNode($"//tr[{i}]").Attributes["data-obj-id"].Value,
                        PeriodName = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[1]//text()").InnerText,
                        Income = income,
                        Spending = spending,
                        StartBalance = startBalance,
                        EndBalance = endBalance
                    });
                }
            }
            catch
            {
                // Если что-то пошло не так, то возвращаем хоть что-нибудь
            }

            return result;
        }

        /// <summary>
        /// Получение сгруппированных расходов на услуги в указанном месяце
        /// </summary>
        public List<MonthBillGroup> GetMonthlyDetails(string accountPath, IRestClient client, string accountId, MonthBillSummary monthlyBillSummary)
        {
            var path = accountPath.Substring(0, accountPath.IndexOf('?'));
            var result = GetMonthBillGroups(
                client,
                path,
                monthlyBillSummary.Id,
                monthlyBillSummary.GetLinkId(GetMonthBillsLinkId, client, path, accountPath, accountId),
                out var groupUrlTemplate);
            lock (FillBillsLockObject)
            {
                Parallel.ForEach(result, item => FillBills(item, client, groupUrlTemplate));
            }

            return result;
        }

        private List<MonthBillGroup> GetMonthBillGroups(
            IRestClient client,
            string path,
            string monthlyBillSummaryId,
            string monthlyBillSummaryLinkId,
            out string groupUrlTemplate)
        {
            var result = new List<MonthBillGroup>();

            var request = new RestRequest($"{path}{MonthlyBillsPath}", Method.GET);
            request.AddParameter("_parent_id", monthlyBillSummaryId);
            request.AddParameter("__ilink_id__", monthlyBillSummaryLinkId);
            request.AddParameter("simple", 1);

            var response = client.Execute(request);
            var content = Regex.Replace(response.Content, @"\\t|\\n|\\r|\\", "");
            var tbody = GetTagValue(content, "<tbody>", "</tbody>");
            var document = new HtmlDocument();
            document.LoadHtml(tbody);

            try
            {
                var i = 0;
                while (true)
                {
                    i++;
                    if (string.IsNullOrWhiteSpace(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[1]//text()")?.InnerText))
                    {
                        break;
                    }

                    decimal.TryParse(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[7]//text()").InnerText.Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var amount);
                    decimal.TryParse(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[8]//text()").InnerText.Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var cost);
                    decimal.TryParse(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[9]//text()").InnerText.Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var costWithTax);
                    result.Add(new MonthBillGroup
                    {
                        Id = EncodeRowId(document.DocumentNode.SelectSingleNode($"//tr[{i}]").Attributes["data-obj-id"].Value),
                        PeriodName = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[1]//text()").InnerText,
                        LinkId = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[2]//text()").InnerText,
                        TariffPlan = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[4]//text()").InnerText,
                        ServiceName = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[5]//text()").InnerText,
                        UnitOfMeasurement = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[6]//text()").InnerText,
                        Amount = amount,
                        Cost = cost,
                        CostWithTax = costWithTax
                    });
                }
            }
            catch
            {
                // Если что-то пошло не так, то возвращаем хоть что-нибудь
            }

            groupUrlTemplate = GetAttributeValue(GetTagValue(content, "<form", "</form>"), "action=\"");

            return result;
        }

        private static string GetMonthBillsLinkId(IRestClient client, string path, string accountPath, string monthlyBillSummaryId, string accountId)
        {
            var request = new RestRequest($"{path}{monthlyBillSummaryId}/", Method.GET);
            request.AddParameter(IlinkIdParamName, GetParamValue(accountPath, IlinkIdParamName));
            request.AddParameter("__parent_obj__", accountId);
            request.AddParameter("_parent_id", GetParamValue(accountPath, ParentIdParamName));
            request.AddParameter("simple", 1);

            var response = client.Execute(request);
            var ul = new HtmlDocument();
            ul.LoadHtml(GetTagValue(response.Content, "<ul class=\\\"nav nav-tabs\\\">", "</ul>"));
            var link = ul.DocumentNode.SelectSingleNode("/ul[1]/li[2]/a[1]")?.Attributes["href"]?.Value;
            if (!string.IsNullOrWhiteSpace(link))
            {
                link = link.Replace("\\n", "").Replace("\\\"", "");
            }

            return GetParamValue(link, InfoService.IlinkIdParamName);
        }

        private static void FillBills(MonthBillGroup item, IRestClient client, string groupUrlTemplate)
        {
            var url = groupUrlTemplate.Replace("list-action", item.Id);
            var request = new RestRequest($"{url}&simple=1", Method.GET);
            var response = client.Execute(request);
            var content = Regex.Replace(response.Content, @"\\t|\\n|\\r|\\", "");
            var groupDetailsUrl = GetAttributeValue(content, "<li class=\"nav-item\"><a href=\"");

            request = new RestRequest($"{groupDetailsUrl}&simple=1", Method.GET);
            response = client.Execute(request);
            content = Regex.Replace(response.Content, @"\\t|\\n|\\r|\\", "");
            var tbody = GetTagValue(content, "<tbody>", "</tbody>");
            var document = new HtmlDocument();
            document.LoadHtml(tbody);
            item.Bills.AddRange(GetBills(document, item.ServiceName));

            try
            {
                var paginator = GetTagValue(content, "<div class=\"paginatorview\">", "</div>");
                var paginatorDocument = new HtmlDocument();
                paginatorDocument.LoadHtml(paginator);
                var i = 0;
                while (true)
                {
                    i++;
                    if (string.IsNullOrWhiteSpace(paginatorDocument.DocumentNode
                        .SelectSingleNode($"//span[1]//a[{i}]")?.Attributes["href"]?.Value))
                    {
                        break;
                    }

                    var tabUrl = paginatorDocument.DocumentNode
                        .SelectSingleNode($"//span[1]//a[{i}]")?.Attributes["href"]?.Value;
                    var tabRequest = new RestRequest(tabUrl, Method.POST);
                    var tabResponse = client.Execute(tabRequest);
                    var tabContent = Regex.Replace(tabResponse.Content, @"\\t|\\n|\\r|\\", "");
                    var tabbody = GetTagValue(tabContent, "<tbody>", "</tbody>", 2);
                    var tabDocument = new HtmlDocument();
                    tabDocument.LoadHtml(tabbody);
                    item.Bills.AddRange(GetBills(tabDocument, item.ServiceName));
                }
            }
            catch
            {
                // Если что-то пошло не так, то возвращаем хоть что-нибудь
            }

            item.Bills =
                item.Bills.Where(x => x.Period.Year == item.Period.Year && x.Period.Month == item.Period.Month).ToList();
        }

        private static IEnumerable<Bill> GetBills(HtmlDocument document, string serviceName)
        {
            var result = new List<Bill>();

            try
            {
                var i = 0;
                while (true)
                {
                    i++;
                    if (string.IsNullOrWhiteSpace(document.DocumentNode
                        .SelectSingleNode($"//tr[{i}]//td[1]//text()")?.InnerText))
                    {
                        break;
                    }
                    var isServicePay = serviceName.ToLower().Contains("ежемесячный");
                    decimal.TryParse(
                        document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[{(isServicePay ? 7 : 10)}]//text()")?.InnerText
                            .Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var amount);
                    decimal.TryParse(
                        document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[{(isServicePay ? 8 : 11)}]//text()")?.InnerText
                            .Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var cost);
                    decimal.TryParse(
                        document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[{(isServicePay ? 9 : 12)}]//text()")?.InnerText
                            .Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var costWithTax);

                    result.Add(new Bill
                    {
                        Id = document.DocumentNode.SelectSingleNode($"//tr[{i}]").Attributes["data-obj-id"]?.Value,
                        FullPeriodName = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[{(isServicePay ? 2 : 3)}]//text()")?.InnerText,
                        ExitPoint = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[4]//text()")?.InnerText,
                        EntryPoint = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[5]//text()")?.InnerText,
                        ForeigtPointComment = !isServicePay
                            ? document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[8]//text()")?.InnerText
                            : string.Empty,
                        PAN = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[{(isServicePay ? 6 : 7)}]//text()")?.InnerText,
                        CarClass = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[9]//text()")?.InnerText,
                        Amount = amount,
                        Cost = cost,
                        CostWithTax = costWithTax,
                        IsServicePay = isServicePay
                    });
                }
            }
            catch
            {
                // Если что-то пошло не так, то возвращаем хоть что-нибудь
            }

            return result;
        }
    }
}
