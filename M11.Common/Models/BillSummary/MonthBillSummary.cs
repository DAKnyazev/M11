namespace M11.Common.Models.BillSummary
{
    /// <summary>
    /// Общая статистика расходов за месяц
    /// </summary>
    public class MonthBillSummary : BaseMonthBill
    {
        /// <summary>
        /// Идентификатор месяца
        /// </summary>
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
    }
}
