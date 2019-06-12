using System;
using System.Globalization;
using SQLite;

namespace M11.Common.Models.BillSummary
{
    public abstract class BaseMonthBill
    {
        private const string PeriodNameFormat = "yyyy.MM";

        /// <summary>
        /// Рассматриваемый месяц и год в формате 20**.**
        /// </summary>
        [Ignore]
        public string PeriodName
        {
            get => Period.ToString(PeriodNameFormat);
            set
            {
                if (DateTime.TryParseExact(value, PeriodNameFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var period))
                {
                    Period = period;
                }
            }
        }

        /// <summary>
        /// Соответствует ли текущий период указанной дате
        /// </summary>
        public bool IsPeriodEquals(DateTime dateToCompare)
        {
            return string.Equals(PeriodName, dateToCompare.ToString(PeriodNameFormat));
        }

        /// <summary>
        /// Рассматриваемый месяц и год
        /// </summary>
        public DateTime Period { get; set; }
    }
}
