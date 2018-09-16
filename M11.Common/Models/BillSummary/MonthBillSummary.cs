using System;
using System.Globalization;

namespace M11.Common.Models.BillSummary
{
    /// <summary>
    /// Общая статистика расходов за месяц
    /// </summary>
    public class MonthBillSummary
    {
        /// <summary>
        /// Идентификатор месяца
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Рассматриваемый месяц и год в формате 20**.**
        /// </summary>
        public string PeriodName
        {
            get => Period.ToString("yyyy.MM");
            set
            {
                if (DateTime.TryParseExact(value, "yyyy.MM", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var period))
                {
                    Period = period;
                }
            }
        }

        /// <summary>
        /// Рассматриваемый месяц и год
        /// </summary>
        public DateTime Period { get; set; }

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
    }
}
