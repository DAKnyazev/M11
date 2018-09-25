using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using M11.Common.Enums;
using M11.Common.Models;
using M11.Common.Models.BillSummary;
using Newtonsoft.Json;
using RestSharp;

namespace M11.Services
{
    public class InfoService
    {
        public readonly string BaseUrl = "https://private.15-58m11.ru";
        private readonly string _authPath = "onyma/";
        private readonly string _accountDetailsPath = "onyma/lk/account/";
        private readonly string _loginParameterName = "login";
        private readonly string _passwordParameterName = "password";
        private readonly string _submitParameterName = "submit";
        private readonly string _submitParameterValue = "Вход";

        private readonly string _dataObjectIdAttributeName = "data-obj-id=\"";
        private readonly string _accountIdAttributeName = "data-tree-id=\"";
        private readonly string _partyIdParamName = "_party_id=";
        private readonly string _ilinkIdParamName = "__ilink_id__=";

        private static readonly Dictionary<string, string> RowIdEncodeDictionary = new Dictionary<string, string>
        {
            { "$", "$00" },
            { "/", "$01" },
            { "+", "$02" },
            { "&", "$03" },
            { ",", "$04" },
            { ":", "$05" },
            { ";", "$06" },
            { "=", "$07" },
            { "?", "$08" },
            { "@", "$09" }
        };

        /// <summary>
        /// Получение информации о договоре клиента
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        public Info GetInfo(string login, string password)
        {
            try
            {
                var cookieContainer = new CookieContainer();
                var client = new RestClient(BaseUrl) { CookieContainer = cookieContainer };
                var request = new RestRequest($"{_authPath}", Method.POST);
                request.AddParameter(_loginParameterName, login);
                request.AddParameter(_passwordParameterName, password);
                request.AddParameter(_submitParameterName, _submitParameterValue);
                var response = client.Execute(request);

                var stringContent = response.Content;

                if (string.IsNullOrEmpty(stringContent))
                {
                    return new Info();
                }

                var commonTable = GetTagValue(stringContent, "<table class=\"infoblock fullwidth\">", "</table>");
                var commonInfoDocument = new HtmlDocument();
                commonInfoDocument.LoadHtml(commonTable);
                var ticketsSpan = GetTagValue(stringContent, "<span style=\"\" class=\"w-html-ro\">", "</span>", 3)
                    ?.Replace("&nbsp;", string.Empty);
                var ticketsDocument = new HtmlDocument();
                ticketsDocument.LoadHtml(ticketsSpan);
                var linkTable = GetTagValue(stringContent, "<div class=\"tmenu\">", "</div>");
                var linkDocument = new HtmlDocument();
                linkDocument.LoadHtml(linkTable);

                return new Info
                {
                    RequestDate = DateTime.Now,
                    ContractNumber =
                        commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[1]//td[2]//text()").InnerText,
                    Phone = Regex.Replace(GetTagValue(stringContent, "<span class=\"w-text-ro\">", "</span>"), "[^+0-9.]", ""),
                    Status = commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[2]//td[2]//text()").InnerText,
                    Balance = commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[3]//td[2]//text()").InnerText,
                    Tickets = GetTickets(ticketsDocument),
                    Links = GetLinks<LinkType>(linkDocument, "//tr[1]//td[{0}]//a[1]"),
                    CookieContainer = cookieContainer
                };
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch
            {
                // Скорее всего какая-то ошибка парсинга
                return new Info();
            }
        }

        /// <summary>
        /// Получение данных со страницы "Лицевой счёт"
        /// </summary>
        /// <param name="path">относительный путь</param>
        /// <param name="cookieContainer">Коллекция куки, которая нужна для запроса</param>
        /// <param name="start">Дата начала периода</param>
        /// <param name="end">Дата окончания периода</param>
        public AccountInfo GetAccountInfo(string path, CookieContainer cookieContainer, DateTime start, DateTime end)
        {
            var result = new AccountInfo { RestClient = new RestClient(BaseUrl) { CookieContainer = cookieContainer } };
            var request = new RestRequest($"{path}", Method.GET);
            var response = result.RestClient.Execute(request);
            result.DataObjectId = EncodeRowId(GetAttributeValue(response.Content, _dataObjectIdAttributeName));
            result.AccountId = GetAttributeValue(response.Content, _accountIdAttributeName);
            result.PartyId = GetParamValue(path, _partyIdParamName);
            result.IlinkId = GetParamValue(path, _ilinkIdParamName);
            var accountRequest = new RestRequest(
                $"{_accountDetailsPath}{result.DataObjectId}/?__ilink_id__={result.IlinkId}&__parent_obj__={result.PartyId}&_party_id={result.PartyId}&simple=1",
                Method.GET);
            var accountResponse = result.RestClient.Execute(accountRequest);
            var accountLinksDiv = GetTagValue(accountResponse.Content, "<div class=\\\"links\\\">", "</div>");
            accountLinksDiv = Regex.Replace(accountLinksDiv, @"\t|\n|\r|\\", "");
            var accountLinksDivHtml = new HtmlDocument();
            accountLinksDivHtml.LoadHtml(accountLinksDiv);
            var accountLinks = GetLinks<AccountLinkType>(accountLinksDivHtml, "/div[1]/ul[1]/li[{0}]/a[1]");
            result.BillSummaryList = GetMonthlyStatistic(result.RestClient,
                accountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
                start,
                end);
            result.RequestDate = DateTime.Now;

            return result;
        }

        /// <summary>
        /// Получение содержимого страницы входа в личный кабинет
        /// </summary>
        /// <param name="login">Логин для входа</param>
        /// <param name="password">Пароль для входа</param>
        /// <param name="pageType">Тип класса из сборки, где находиться внедренный файл</param>
        public string GetLoginPageContent(string login, string password, Type pageType)
        {
            var loginPageContent = GetEmbeddedFileContent("LoginPage.html", pageType).Replace("{0}", login).Replace("{1}", password);

            return loginPageContent;
        }

        /// <summary>
        /// Получение содержимого платежной страницы
        /// </summary>
        /// <param name="accountId">Идентификатор аккаунта</param>
        /// <param name="amount">Сумма пополнения</param>
        /// <param name="phone">Телефон в формате +79000000000</param>
        /// <param name="pageType">Тип класса из сборки, где находиться внедренный файл</param>
        public string GetPaymentPageContent(string accountId, int amount, string phone, Type pageType)
        {
            var paymentPageContent = string.Format(GetEmbeddedFileContent("PaymentPage.html", pageType), accountId, amount, phone);

            return paymentPageContent;
        }

        /// <summary>
        /// Получение содержимого тега
        /// </summary>
        private static string GetTagValue(string content, string startingTag, string endingTag, int index = 1)
        {
            var startIndex = 0;
            for (int i = 0; i < index; i++)
            {
                if (i != 0)
                {
                    content = content.Substring(startIndex + 1);
                }

                startIndex = content.IndexOf(startingTag, StringComparison.InvariantCultureIgnoreCase);
            }
            
            if (startIndex > 0)
            {
                var endIndex = content.IndexOf(endingTag, startIndex, StringComparison.InvariantCultureIgnoreCase);

                return content.Substring(startIndex, endIndex - startIndex + endingTag.Length);
            }

            return string.Empty;
        }

        /// <summary>
        /// Получение значения аттрибута
        /// </summary>
        private static string GetAttributeValue(string content, string attributeName)
        {
            var startIndex = content.IndexOf(attributeName, StringComparison.InvariantCultureIgnoreCase);
            var tmp = content.Substring(startIndex + attributeName.Length);
            var endIndex = tmp.IndexOf("\"", StringComparison.InvariantCulture);

            return tmp.Substring(0, endIndex);
        }

        /// <summary>
        /// Получение значения параметра из пути
        /// </summary>
        private static string GetParamValue(string path, string paramName)
        {
            var startIndex = path.IndexOf(paramName, StringComparison.InvariantCultureIgnoreCase);
            var tmp = path.Substring(startIndex + paramName.Length);
            var endIndex = tmp.IndexOf("&", StringComparison.InvariantCulture);

            return endIndex < 0 ? tmp : tmp.Substring(0, endIndex);
        }

        /// <summary>
        /// Закодировать идентификатор строки аккаунта
        /// </summary>
        private static string EncodeRowId(string rowId)
        {
            foreach (var item in RowIdEncodeDictionary)
            {
                rowId = rowId.Replace(item.Key, item.Value);
            }

            return rowId;
        }

        /// <summary>
        /// Получить список абонементов
        /// </summary>
        private static List<Ticket> GetTickets(HtmlDocument span)
        {
            var result = new List<Ticket>();
            try
            {
                var i = 1;
                while (true)
                {
                    i++;
                    if (string.IsNullOrWhiteSpace(span.DocumentNode.SelectSingleNode($"//tr[{i}]").InnerText))
                    {
                        break;
                    }

                    DateTime.TryParseExact(span.DocumentNode.SelectSingleNode($"//tr[{i}]//td[6]//text()").InnerText,
                        "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate);
                    DateTime.TryParseExact(span.DocumentNode.SelectSingleNode($"//tr[{i}]//td[7]//text()").InnerText,
                        "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiryDate);
                    int.TryParse(span.DocumentNode.SelectSingleNode($"//tr[{i}]//td[8]//text()").InnerText,
                        out var remainingTripsCount);

                    result.Add(new Ticket
                    {
                        ContractNumber = span.DocumentNode.SelectSingleNode($"//tr[{i}]//td[1]//text()").InnerText,
                        TransponderNumber = span.DocumentNode.SelectSingleNode($"//tr[{i}]//td[2]//text()").InnerText,
                        Description = span.DocumentNode.SelectSingleNode($"//tr[{i}]//td[5]//text()").InnerText,
                        StartDate = startDate,
                        ExpiryDate = expiryDate,
                        RemainingTripsCount = remainingTripsCount,
                        Status = span.DocumentNode.SelectSingleNode($"//tr[{i}]//td[9]//text()").InnerText
                    });
                }
            }
            catch
            {
                // Ну не смогли, так не смогли
            }

            return result;
        }

        /// <summary>
        /// Получить список основных ссылок
        /// </summary>
        private static List<Link<TLinkType>> GetLinks<TLinkType>(HtmlDocument table, string xpath) where TLinkType : Enum
        {
            var result = new List<Link<TLinkType>>();
            try
            {
                var i = 0;
                while (true)
                {
                    i++;
                    var nodeXpath = string.Format(xpath, i);
                    if (string.IsNullOrWhiteSpace(table.DocumentNode.SelectSingleNode(nodeXpath)
                        .InnerText))
                    {
                        break;
                    }

                    result.Add(new Link<TLinkType>
                    {
                        Type = (TLinkType) Enum.Parse(typeof(TLinkType), i.ToString(), true),
                        RelativeUrl = table.DocumentNode.SelectSingleNode(nodeXpath).Attributes["href"].Value
                    });
                }
            }
            catch
            {

            }

            return result;
        }

        /// <summary>
        /// Получение статистики расходов по месяцам
        /// </summary>
        private static List<MonthBillSummary> GetMonthlyStatistic(RestClient client, string path, DateTime start, DateTime end)
        {
            var result = new List<MonthBillSummary>();
            var request = new RestRequest(path + "&simple=1", Method.POST);
            var param = JsonConvert.SerializeObject(new List<BillsSummaryParam> { new BillsSummaryParam(start, end) });
            request.AddParameter("raw_state", param);
            var response = client.Execute(request);
            var content = Regex.Replace(response.Content, @"\t|\n|\r|\\", "");
            var tbody = GetTagValue(content, "<tbody>", "</tbody>");
            var document = new HtmlDocument();
            document.LoadHtml(tbody);
            try
            {
                var i = 0;
                while (true)
                {
                    i++;
                    if (string.IsNullOrWhiteSpace(document.DocumentNode.SelectSingleNode($"//tr[{i}]//td[1]//text()").InnerText))
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
            }

            return result;
        }

        /// <summary>
        /// Получение контента внедренного файла (unit-test или андроид)
        /// </summary>
        /// <param name="fileName">Имя внедренного файла</param>
        /// <param name="assemblyType">Тип из сборки</param>
        /// <returns></returns>
        private static string GetEmbeddedFileContent(string fileName, Type assemblyType)
        {
            var assembly = assemblyType.GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream($"M11.Resources.{fileName}"))
            {
                if (stream == null)
                {
                    using (var androidAssembly = assembly.GetManifestResourceStream($"M11.Android.Resources.{fileName}"))
                    {
                        if (androidAssembly == null)
                        {
                            return string.Empty;
                        }
                        using (var reader = new StreamReader(androidAssembly))
                        {
                            return reader.ReadToEnd();
                        }
                    }

                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
