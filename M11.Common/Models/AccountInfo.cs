﻿using System.Collections.Generic;
using M11.Common.Enums;
using M11.Common.Models.BillSummary;
using RestSharp;

namespace M11.Common.Models
{
    /// <summary>
    /// Информация о лицевом счёте
    /// </summary>
    public class AccountInfo : BaseInfo
    {
        /// <summary>
        /// Клиент, для запросов по лицевому счёту
        /// </summary>
        public RestClient RestClient { get; set; }

        /// <summary>
        /// Идентификатор DataObjectId, для запросов по лицевому счёту
        /// </summary>
        public string DataObjectId { get; set; }

        /// <summary>
        /// Идентификатор аккаунта
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Идентификатор PartyId, для запросов по лицевому счёту
        /// </summary>
        public string PartyId { get; set; }

        /// <summary>
        /// Идентификатор IlinkId, для запросов по лицевому счёту
        /// </summary>
        public string IlinkId { get; set; }

        /// <summary>
        /// Список статистика расходов по месяцам
        /// </summary>
        public List<MonthBillSummary> BillSummaryList { get; set; }

        /// <summary>
        /// Ссылки на основные страницы
        /// </summary>
        public List<Link<AccountLinkType>> AccountLinks { get; set; }
    }
}
