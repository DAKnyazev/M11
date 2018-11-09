using System;
using System.Globalization;

namespace M11.Common.Models.BillSummary
{
    public abstract class BaseBill : BaseMonthBill
    {
        /// <summary>
        /// Количество
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Стоимость
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Стоимость с НДС
        /// </summary>
        public decimal CostWithTax { get; set; }

        /// <summary>
        /// Рассматриваемый месяц и год в формате yyyy.MM.dd HH:mm:ss
        /// </summary>
        public string FullPeriodName
        {
            get => Period.ToString("yyyy.MM.dd HH:mm:ss");
            set
            {
                if (DateTime.TryParseExact(value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var period))
                {
                    Period = period;
                }
            }
        }
    }
}
