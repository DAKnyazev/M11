using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public async Task<Info> GetParticipantInfo(string login, string password)
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
                var table = GetTagValue(stringContent, "<table class=\"infoblock fullwidth\">", "</table>");
                var values = GetInfoValus(table);
                if (values == null)
                {
                    return new Info();
                }

                return new Info
                {
                    ContractNumber = values.Any(x => x.Item1 == "Договор") ? values.First(x => x.Item1 == "Договор").Item2 : string.Empty,
                    Status = values.Any(x => x.Item1 == "Статус") ? values.First(x => x.Item1 == "Статус").Item2 : string.Empty,
                    Balance = values.Any(x => x.Item1 == "Баланс") ? values.First(x => x.Item1 == "Баланс").Item2 : string.Empty
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

        private static IList<Tuple<string, string>> GetInfoValus(string tableInnerHtml)
        {
            var trRegex = new Regex(@"<tr.*?>(.*?)</tr>", RegexOptions.Singleline);
            var tdRegex = new Regex(@"<td.*?>(.*?)</td>", RegexOptions.Singleline);
            var result = new List<Tuple<string, string>>();

            foreach (var tr in trRegex.Matches(tableInnerHtml))
            {
                var tds = tdRegex.Matches(tr.ToString());
                if (tds.Count == 2)
                {
                    var key = tds[0].Groups[1].ToString();
                    var value = tds[1].Groups[1].ToString();
                    result.Add(new Tuple<string, string>(key, value));
                }
            }

            return result;
        }
    }
}
