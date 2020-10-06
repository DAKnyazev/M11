using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public const string Url = "https://m11-neva.ru";
        public const string TariffsTreeVariableName = "var tariffs_tree = ";
        public const string DictionariesVariableName = "var dictionaries = ";
        public const string AutodorPointsVariableName = "var points_avtodor = ";
        public const string PointsVariableName = "var points = ";

        /// <summary>
        /// Дерево тарифов
        /// </summary>
        public static JObject Tariffs { get; set; }

        /// <summary>
        /// Дерево справочников
        /// </summary>
        public static JObject Dictionaries { get; set; }

        /// <summary>
        /// Список пунктов оплаты Автодора
        /// </summary>
        public static List<string> AutodorPoints { get; set; }

        /// <summary>
        /// Список пунктов оплаты М11 15-58
        /// </summary>
        public static List<string> Points { get; set; }

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
                AutodorPoints = JArray.Parse(GetJson(stringContent, AutodorPointsVariableName)).ToObject<List<string>>();
                Points = JArray.Parse(GetJson(stringContent, PointsVariableName)).ToObject<List<string>>();
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
        public CalculatorResult Calculate(string category, string dayweek, string time, string from, string to)
        {
            try
            {
                var tariff = Tariffs[category][dayweek][time][from][to];
                if (tariff == null || tariff.Count() != 2)
                {
                    return CalculateCompositeRoute(category, dayweek, time, from, to);
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

        private CalculatorResult CalculateCompositeRoute(string category, string dayweek, string time, string from, string to)
        {
            JToken tariff1 = null;
            JToken tariff2 = null;

            if (Points.Contains(from) && AutodorPoints.Contains(to))
            {
                tariff1 = Tariffs[category][dayweek][time][from][Points.Last().ToString()];
                tariff2 = Tariffs[category][dayweek][time][AutodorPoints.First().ToString()][to];
            } 
            else if (AutodorPoints.Contains(from) && Points.Contains(to))
            {
                tariff1 = Tariffs[category][dayweek][time][AutodorPoints.First().ToString()][from];
                tariff2 = Tariffs[category][dayweek][time][Points.Last().ToString()][to];
            }
            else
            {
                return new CalculatorResult();
            }

            var costs1 = tariff1.Children().Select(x => decimal.Parse(x.Last.Value<string>())).ToList();
            var costs2 = tariff2.Children().Select(x => decimal.Parse(x.Last.Value<string>())).ToList();

            return new CalculatorResult
            {
                CashCost = costs1.Max() + costs2.Max(),
                TransponderCost = costs1.Min() + costs2.Min(),
                IsComposite = true
            };
        }

        public Dictionary<string, string> GetDestinations()
        {
            return GetDictionary("tos");
        }

        public Dictionary<string, string> GetDepartures()
        {
            return GetDictionary("froms");
        }

        public List<string> GetDayWeeks()
        {
            return Dictionaries["dayweeks"].Children<JProperty>().Select(x => x.Name).ToList();
        }

        public List<Tuple<int, int, string>> GetTimes()
        {
            return Dictionaries["times"].Children<JProperty>().Select(x =>
                new Tuple<int, int, string>(int.Parse(x.Name.Substring(0, 2)), int.Parse(x.Name.Substring(4, 2)), x.Name)).ToList();
        }

        private static Dictionary<string, string> GetDictionary(string name)
        {
            return Dictionaries[name].Children<JProperty>().ToDictionary(x => x.Last.First.Last.ToString(), y => y.Name);
        }

        /// <summary>
        /// Получаем значение переменной с json из JS
        /// </summary>
        /// <param name="content"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        private static string GetJson(string content, string variableName)
        {
            var treeStartIndex = content.IndexOf(variableName, StringComparison.InvariantCultureIgnoreCase) + variableName.Length;
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
