using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using M11.Common.Models;

namespace M11.Services
{
    public class InfoService
    {
        private string _baseUrl = "https://private.15-58m11.ru";
        private string _authPath = "onyma/";
        private string _loginParameterName = "login";
        private string _passwordParameterName = "password";
        private string _submitParameterName = "submit";
        private string _submitParameterValue = "Вход";

        /// <summary>
        /// Получение информации о договоре клиента
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        public async Task<Info> GetInfo(string login, string password)
        {
            try
            {
                var client = new HttpClient();
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>(_loginParameterName, login),
                    new KeyValuePair<string, string>(_passwordParameterName, password),
                    new KeyValuePair<string, string>(_submitParameterName, _submitParameterValue)
                });
                var response = await client.PostAsync($"{_baseUrl}/{_authPath}", formContent);

                var stringContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(stringContent))
                {
                    return new Info();
                }

                var commonTable = GetTagValue(stringContent, "<table class=\"infoblock fullwidth\">", "</table>");
                var commonInfoDocument = new HtmlDocument();
                commonInfoDocument.LoadHtml(commonTable);
                var ticketsSpan = GetTagValue(stringContent, "<span style=\"\" class=\"w-html-ro\">", "</span>")
                    ?.Replace("&nbsp;", string.Empty);
                var ticketsDocument = new HtmlDocument();
                ticketsDocument.LoadHtml(ticketsSpan);

                return new Info
                {
                    ContractNumber =
                        commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[1]//td[2]//text()").InnerText,
                    Status = commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[2]//td[2]//text()").InnerText,
                    Balance = commonInfoDocument.DocumentNode.SelectSingleNode(@"//tr[3]//td[2]//text()").InnerText,
                    Tickets = GetTickets(ticketsDocument)
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

        private static string GetTagValue(string content, string startingTag, string endingTag)
        {
            var startIndex = content.IndexOf(startingTag, StringComparison.InvariantCultureIgnoreCase);
            if (startIndex > 0)
            {
                //startIndex += startingTag.Length;
                var endIndex = content.IndexOf(endingTag, startIndex, StringComparison.InvariantCultureIgnoreCase);

                return content.Substring(startIndex, endIndex - startIndex + endingTag.Length);
            }

            return string.Empty;
        }

        private static List<Ticket> GetTickets(HtmlDocument span)
        {
            return new List<Ticket>();
        }
    }
}
