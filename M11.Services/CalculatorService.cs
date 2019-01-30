using System;
using System.Linq;
using System.Net;
using M11.Common.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace M11.Services
{
    public class CalculatorService
    {
        public const string Url = "https://www.15-58m11.ru/";
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

        public bool TryLoad()
        {
            if (Tariffs != null && Dictionaries != null)
            {
                return true;
            }

            try
            {
                var client = new RestClient(Url);
                var request = new RestRequest(Method.GET);
                var response = client.Execute(request);
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

        private static string GetJson(string content, string variableName)
        {
            var treeStartIndex = content.IndexOf(variableName, StringComparison.InvariantCultureIgnoreCase) + TariffsTreeVariableName.Length;
            var treeEndIndex = content.IndexOf(";", treeStartIndex, StringComparison.InvariantCultureIgnoreCase);
            return content.Substring(treeStartIndex, treeEndIndex - treeStartIndex);
        }
    }
}
