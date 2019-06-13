using System;
using System.Collections.Generic;
using RestSharp;
using SQLite;

namespace M11.Common.Models.BillSummary
{
    /// <summary>
    /// Общая статистика расходов за месяц
    /// </summary>
    [Table("MonthBillSummary")]
    public class MonthBillSummary : BaseMonthBill, IDatabaseEntity
    {
        /// <summary>
        /// Идентификатор общей статистики расходов
        /// </summary>
        private static string _linkId = string.Empty;

        public MonthBillSummary()
        {
            Groups = new List<MonthBillGroup>();
        }

        /// <summary>
        /// Идентификатор месяца
        /// </summary>
        [PrimaryKey]
        public string Id { get; set; }

        /// <summary>
        /// Приход
        /// </summary>
        public decimal Income { get; set; }

        /// <summary>
        /// Расход
        /// </summary>
        public decimal Spending { get; set; }

        /// <summary>
        /// Остаток на начало
        /// </summary>
        public decimal StartBalance { get; set; }

        /// <summary>
        /// Остаток на конец
        /// </summary>
        public decimal EndBalance { get; set; }

        /// <summary>
        /// Группы расходов
        /// </summary>
        [Ignore]
        public List<MonthBillGroup> Groups { get; set; }

        /// <summary>
        /// Дата запроса групп расходов
        /// </summary>
        [Ignore]
        public DateTime GroupsRequestDate { get; set; }

        /// <summary>
        /// Получить идентификатор общей статистики расходов
        /// </summary>
        public string GetLinkId(
            Func<IRestClient, string, string, string, string, string> linkIdFunc,
            IRestClient client,
            string path, 
            string accountPath, 
            string accountId)
        {
            lock (_linkId)
            {
                if (string.IsNullOrWhiteSpace(_linkId))
                {
                    _linkId = linkIdFunc(client, path, accountPath, Id, accountId);
                }
            }

            return _linkId;
        }
    }
}
