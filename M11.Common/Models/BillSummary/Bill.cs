using SQLite;

namespace M11.Common.Models.BillSummary
{
    [Table("Bill")]
    public class Bill : BaseBill, IDatabaseEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [PrimaryKey]
        public string Id { get; set; }

        /// <summary>
        /// Идентификатор группы
        /// </summary>
        /// <remarks>Внешний ключ к MonthBillGroup</remarks>
        public string MonthBillGroupId { get; set; }

        /// <summary>
        /// ПВП Въезда
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// ПВП выезда
        /// </summary>
        public string ExitPoint { get; set; }

        /// <summary>
        /// Комментарий, в каком пункте мы въехали или выехали, если он был за пределами участка 15-58 км М-11
        /// </summary>
        public string ForeigtPointComment { get; set; }

        /// <summary>
        /// PAN
        /// </summary>
        public string PAN { get; set; }

        /// <summary>
        /// Класс ТС
        /// </summary>
        public string CarClass { get; set; }

        /// <summary>
        /// Это сервисный сбор? (Ежемесячный платеж)
        /// </summary>
        public bool IsServicePay { get; set; }

        /// <summary>
        /// Это покупка абонемента?
        /// </summary>
        [Ignore]
        public bool IsTicketBuy => string.IsNullOrWhiteSpace(EntryPoint) && string.IsNullOrWhiteSpace(ExitPoint);
    }
}
