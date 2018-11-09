using System;
using System.Globalization;

namespace M11.Common.Models.BillSummary
{
    public abstract class BaseMonthBill
    {
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
    }
}
