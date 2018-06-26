using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using M11.Common.Models;

namespace M11.Services
{
    public class AuthService
    {
        private string _baseUrl = "https://private.15-58m11.ru";
        private string _authPath = "onyma/";
        private string _loginParameterName = "login";
        private string _passwordParameterName = "password";
        private string _submitParameterName = "submit";
        private string _submitParameterValue = "Вход";

        public Info GetParticipantInfo(string login, string password)
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
                var response = client.PostAsync($"{_baseUrl}/{_authPath}", formContent).Result;

                var stringContent = response.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(stringContent))
                {
                    return new Info();
                }

                var values = GetInfoValus(GetTagValue(stringContent, "<table class=\"infoblock fullwidth\">", "</table>"));
                if (values == null)
                {
                    return new Info();
                }

                return new Info
                {
                    ContractNumber = values.Any(x => x.Key == "Договор") ? values.First(x => x.Key == "Договор").Value : string.Empty,
                    Status = values.Any(x => x.Key == "Статус") ? values.First(x => x.Key == "Статус").Value : string.Empty,
                    Balance = values.Any(x => x.Key == "Баланс") ? values.First(x => x.Key == "Баланс").Value : string.Empty
                };
            }
            catch (Exception e)
            {
                var message = e.Message;
                throw;
            }
        }

        private static string GetTagValue(string content, string startingTag, string endingTag)
        {
            var startIndex = content.IndexOf(startingTag, StringComparison.InvariantCultureIgnoreCase);
            if (startIndex > 0)
            {
                startIndex += startingTag.Length;
                var endIndex = content.IndexOf(endingTag, startIndex, StringComparison.InvariantCultureIgnoreCase);

                return content.Substring(startIndex, endIndex - startIndex);
            }

            return string.Empty;
        }

        private static IList<KeyValuePair<string, string>> GetInfoValus(string tableInnerHtml)
        {
            var trRegex = new Regex(@"<tr.*?>(.*?)</tr>", RegexOptions.Singleline);
            var tdRegex = new Regex(@"<td.*?>(.*?)</td>", RegexOptions.Singleline);
            var result = new List<KeyValuePair<string, string>>();

            foreach (var tr in trRegex.Matches(tableInnerHtml))
            {
                var tds = tdRegex.Matches(tr.ToString());
                if (tds.Count == 2)
                {
                    result.Add(new KeyValuePair<string, string>(tds[0].Groups[0].ToString(), tds[1].Groups[0].ToString()));
                }
            }

            return result;
        }
    }
}
