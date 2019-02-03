using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using M11.Common.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace M11.Services
{
    public class CalculatorService
    {
        public const string Url = "https://www.15-58m11.ru";
        public const string TariffsTreeVariableName = "var tariffs_tree = ";
        public const string DictionariesVariableName = "var dictionaries = ";

        /// <summary>
        /// Дерево тарифов
        /// </summary>
        public static JObject Tariffs { get; set; }

        /// <summary>
        /// Дерево справочников
        /// </summary>
        public static JObject Dictionaries { get; set; }
        
        /// <summary>
        /// Попытаться загрузить справочники для калькулятора
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TryLoadAsync()
        {
            if (Tariffs != null && Dictionaries != null)
            {
                return true;
            }

            try
            {
                var client = new RestClient(Url);
                var request = new RestRequest(Method.GET);

                ServicePointManager.ServerCertificateValidationCallback += OnServerCertificateValidationCallback;
                var cancellationTokenSource = new CancellationTokenSource();
                var response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }
                var stringContent = response.Content;
                Tariffs = JObject.Parse(GetJson(stringContent, TariffsTreeVariableName));
                Dictionaries = JObject.Parse(GetJson(stringContent, DictionariesVariableName));
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Рассчитать стоимость
        /// </summary>
        public CalculatorResult Calculate(int category)
        {
            try
            {
                var tariff = Tariffs[category.ToString()]["chetverg"]["00000100"]["moskva"]["klin89ykm"];
                if (tariff == null || tariff.Count() != 2)
                {
                    return new CalculatorResult();
                }

                var costs = tariff.Children().Select(x => decimal.Parse(x.Last.Value<string>())).ToList();

                return new CalculatorResult
                {
                    CashCost = costs.Max(),
                    TransponderCost = costs.Min()
                };
            }
            catch (Exception e)
            {
                return new CalculatorResult();
            }
        }

        /// <summary>
        /// Получаем значение переменной с json из JS
        /// </summary>
        /// <param name="content"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        private static string GetJson(string content, string variableName)
        {
            var treeStartIndex = content.IndexOf(variableName, StringComparison.InvariantCultureIgnoreCase) + TariffsTreeVariableName.Length;
            var treeEndIndex = content.IndexOf(";", treeStartIndex, StringComparison.InvariantCultureIgnoreCase);
            return content.Substring(treeStartIndex, treeEndIndex - treeStartIndex);
        }

        /// <summary>
        /// Обработка результата проверки сертификата
        /// </summary>
        /// <remarks>Используем из-за проблемы с сертификатом сайта https://www.15-58m11.ru</remarks>
        private static bool OnServerCertificateValidationCallback(object sender, X509Certificate cert, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if (sender is HttpWebRequest request && request.Address.AbsoluteUri.Contains(Url))
            {
                return true;
            }

            return false;
        }
    }
}
