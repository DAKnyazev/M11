using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using M11.Common.Models;
using M11.Common.Models.BillSummary;
using Newtonsoft.Json;
using RestSharp;

namespace M11.Services
{
    public class StatisticService : BaseInfoService
    {
        private static readonly object FillBillsLockObject = new object();
        private static readonly string MonthlyBillsPath = "month_bills2/";

        public ListResult<MonthBillSummary> GetMonthlyStatistic(
            IRestClient client, 
            string path, 
            DateTime start,
            DateTime end, 
            string accountId)
        {
            var result = new ListResult<MonthBillSummary>();
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
                result.IsError = true;
                // Если что-то пошло не так, то возвращаем хоть что-нибудь
            }

            return result;
        }

        /// <summary>
        /// Получение сгруппированных расходов на услуги в указанном месяце
        /// </summary>
        public ListResult<MonthBillGroup> GetMonthlyDetails(string accountPath, IRestClient client, string accountId, MonthBillSummary monthlyBillSummary)
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
                Parallel.ForEach(result.List, item => result.IsError = result.IsError || !TryFillBills(item, client, groupUrlTemplate));
            }

            return result;
        }

        private ListResult<MonthBillGroup> GetMonthBillGroups(
            IRestClient client,
            string path,
            string monthlyBillSummaryId,
            string monthlyBillSummaryLinkId,
            out string groupUrlTemplate)
        {
            var result = new ListResult<MonthBillGroup>();

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
                result.IsError = true;
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

        private static bool TryFillBills(MonthBillGroup item, IRestClient client, string groupUrlTemplate)
        {
            var result = true;
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
            var billsResult = GetBills(document, item.ServiceName);
            result = !billsResult.IsError;
            item.Bills.AddRange(billsResult.List);

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

                    var pageIndex = paginatorDocument.DocumentNode
                        .SelectSingleNode($"//span[1]//a[{i}]//text()")?.InnerText;
                    var tabRequest = new RestRequest($"{groupDetailsUrl}&page={pageIndex}&simple=1", Method.POST);
                    var tabResponse = client.Execute(tabRequest);
                    var tabContent = Regex.Replace(tabResponse.Content, @"\\t|\\n|\\r|\\", "");
                    var tabbody = GetTagValue(tabContent, "<tbody>", "</tbody>", 1);
                    var tabDocument = new HtmlDocument();
                    tabDocument.LoadHtml(tabbody);
                    billsResult = GetBills(tabDocument, item.ServiceName);
                    result = result && !billsResult.IsError;
                    item.Bills.AddRange(billsResult.List);
                }
            }
            catch
            {
                result = false;
                // Если что-то пошло не так, то возвращаем хоть что-нибудь
            }

            item.Bills =
                item.Bills.Where(x => x.Period.Year == item.Period.Year && x.Period.Month == item.Period.Month).ToList();
            return result;
        }

        private static ListResult<Bill> GetBills(HtmlDocument document, string serviceName)
        {
            var result = new ListResult<Bill>();

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
                        document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[@data-name='useserv']//text()")?.InnerText
                            .Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var amount);
                    decimal.TryParse(
                        document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[@data-name='amount']//text()")?.InnerText
                            .Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var cost);
                    decimal.TryParse(
                        document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[@data-name='amount_tax']//text()")?.InnerText
                            .Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var costWithTax);
                    var bill = new Bill
                    {
                        Id = document.DocumentNode.SelectSingleNode($"//tr[{i}]").Attributes["data-obj-id"]?.Value,
                        FullPeriodName =
                            document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[@data-name='mdate']//text()")
                                ?.InnerText,
                        ExitPoint = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[@data-name='tdfid']//text()")?.InnerText,
                        EntryPoint = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[@data-name='remark']//text()")?.InnerText,
                        ForeigtPointComment = !isServicePay
                            ? document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[7]//text()")?.InnerText
                            : string.Empty,
                        PAN = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[@data-name='fid']//text()")
                            ?.InnerText,
                        CarClass = document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[@data-name='tdid']//text()")?.InnerText,
                        Amount = amount,
                        Cost = cost,
                        CostWithTax = costWithTax,
                        IsServicePay = isServicePay
                    };
                    result.Add(bill);
                }
            }
            catch
            {
                result.IsError = true;
                // Если что-то пошло не так, то возвращаем хоть что-нибудь
            }

            return result;
        }
    }
}
