﻿using System.Collections.Generic;

namespace M11.Common.Models.BillSummary
{
    /// <summary>
    /// Группа услуг за месяц
    /// </summary>
    public class MonthBillGroup : BaseBill
    {
        public MonthBillGroup()
        {
            Bills = new List<Bill>();
        }

        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Идентификатор ссылки группы
        /// </summary>
        public string LinkId { get; set; }

        /// <summary>
        /// Тарифный план
        /// </summary>
        public string TariffPlan { get; set; }

        /// <summary>
        /// Название услуги
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Единица измерений
        /// </summary>
        public string UnitOfMeasurement { get; set; }

        /// <summary>
        /// Список трат
        /// </summary>
        public List<Bill> Bills { get; set; }
    }
}