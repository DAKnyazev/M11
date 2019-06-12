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
using M11.Common;
using M11.Common.Enums;
using M11.Common.Models;
using M11.Common.Models.BillSummary;
using RestSharp;

namespace M11.Services
{
    public class InfoService : BaseInfoService
    {
        private readonly CachedStatisticService _cachedStatisticService;

        public InfoService(GenericDatabase<MonthBillSummary> monthBillSummaryRepository)
        {
            _cachedStatisticService = new CachedStatisticService(monthBillSummaryRepository);
        }

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
        

        private readonly string _loginPageName = "LoginPage.html";
        private readonly string _paymentPageName = "PaymentPage.html";

        private readonly StatisticService _statisticService = new StatisticService();

        /// <summary>
        /// Получение информации о договоре клиента
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        public AccountBalance GetAccountBalance(string login, string password)
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
                    return new AccountBalance { StatusCode = response.StatusCode };
                }

                var commonTable = GetTagValue(stringContent, "<table class=\"infoblock fullwidth\">", "</table>");
                var commonInfoDocument = new HtmlDocument();
                commonInfoDocument.LoadHtml(commonTable);
                var ticketsSpan = GetTagValue(stringContent, "<span class=\"w-html-ro\" style=\"\">", "</span>", 3)
                    ?.Replace("&nbsp;", string.Empty);
                var ticketsDocument = new HtmlDocument();
                ticketsDocument.LoadHtml(ticketsSpan);
                var linkTable = GetTagValue(stringContent, "<div class=\"tmenu\">", "</div>");
                var linkDocument = new HtmlDocument();
                linkDocument.LoadHtml(linkTable);
                var ticketLinkInput = GetTagValue(stringContent, "<input class=\"c-button\"", ">");

                return new AccountBalance
                {
                    RequestDate = DateTime.Now,
                    ContractNumber =
                        commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[1]//td[2]//text()").InnerText,
                    Phone = Regex.Replace(GetTagValue(stringContent, "<span class=\"w-text-ro", "</span>", 5, false), "[^+0-9.]", ""),
                    Status = commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[2]//td[2]//text()").InnerText,
                    Balance = commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[3]//td[2]//text()").InnerText,
                    Tickets = GetTickets(ticketsDocument),
                    Links = GetLinks<LinkType>(linkDocument, "//tr[1]//td[{0}]//a[1]"),
                    CookieContainer = cookieContainer,
                    TicketLink = GetTicketLink(ticketLinkInput, BaseUrl)
                };
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch
            {
                // Скорее всего какая-то ошибка парсинга
                return new AccountBalance { StatusCode = HttpStatusCode.Unauthorized };
            }
        }

        /// <summary>
        /// Получение данных со страницы "Лицевой счёт"
        /// </summary>
        /// <param name="path">относительный путь</param>
        /// <param name="cookieContainer">Коллекция куки, которая нужна для запроса</param>
        /// <param name="start">Дата начала периода</param>
        /// <param name="end">Дата окончания периода</param>
        /// <param name="accountId">Идентификатор аккаунта</param>
        /// <param name="dataObjectId">Идентификатор DataObjectId, для запросов по лицевому счёту</param>
        public AccountInfo GetAccountInfo(string path, CookieContainer cookieContainer, DateTime start, DateTime end, string accountId = null, string dataObjectId = null)
        {
            var result = new AccountInfo { RestClient = new RestClient(BaseUrl) { CookieContainer = cookieContainer } };
            if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(dataObjectId))
            {
                var request = new RestRequest($"{path}", Method.GET);
                var response = result.RestClient.Execute(request);
                result.AccountId = GetAttributeValue(response.Content, _accountIdAttributeName);
                result.DataObjectId = EncodeRowId(GetAttributeValue(response.Content, _dataObjectIdAttributeName));
            }
            else
            {
                result.AccountId = accountId;
                result.DataObjectId = dataObjectId;
            }
            result.PartyId = GetParamValue(path, _partyIdParamName);
            result.IlinkId = GetParamValue(path, IlinkIdParamName);

            var accountRequest = new RestRequest(
                $"{_accountDetailsPath}{result.DataObjectId}/?__ilink_id__={result.IlinkId}&__parent_obj__={result.PartyId}&_party_id={result.PartyId}&simple=1",
                Method.GET);
            var accountResponse = result.RestClient.Execute(accountRequest);
            var accountLinksDiv = GetTagValue(accountResponse.Content, "<div class=\\\"links\\\">", "</div>");
            accountLinksDiv = Regex.Replace(accountLinksDiv, @"\\t|\\n|\\r|\\", "");
            var accountLinksDivHtml = new HtmlDocument();
            accountLinksDivHtml.LoadHtml(accountLinksDiv);
            result.AccountLinks = GetLinks<AccountLinkType>(accountLinksDivHtml, "/div[1]/ul[1]/li[{0}]/a[1]");
            result.BillSummaryList = _cachedStatisticService.GetMonthlyStatistic(
                result.RestClient,
                result.AccountLinks.FirstOrDefault(x => x.Type == AccountLinkType.Account)?.RelativeUrl,
                start,
                end,
                result.AccountId);
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
            var loginPageContent = GetEmbeddedFileContent(_loginPageName, pageType).Replace("{0}", login).Replace("{1}", password);

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
            var paymentPageContent = string.Format(GetEmbeddedFileContent(_paymentPageName, pageType), accountId, amount, phone);

            return paymentPageContent;
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
                        ?.InnerText))
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
                // Если что-то пошло не так, то возвращаем хоть что-нибудь
            }

            return result;
        }

        /// <summary>
        /// Получение ссылки на оформление абонемента
        /// </summary>
        private static string GetTicketLink(string ticketLinkInput, string baseUrl)
        {
            return baseUrl + GetAttributeValue(ticketLinkInput, "onclick=\"")?.Replace("window.open('", string.Empty).Replace("')", string.Empty);
        }

        /// <summary>
        /// Получение контента внедренного файла (unit-test или андроид)
        /// </summary>
        /// <param name="fileName">Имя внедренного файла</param>
        /// <param name="assemblyType">Тип из сборки</param>
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
