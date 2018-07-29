using System;

namespace M11.Common.Models
{
    /// <summary>
    /// Информация об абонементе
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Номер транспондера
        /// </summary>
        public string TransponderNumber { get; set; }

        /// <summary>
        /// Описание абонемента
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Дата начала использования
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата окончания действия абонемента
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Количество оставшихся поездок
        /// </summary>
        public int RemainingTripsCount { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Активирован ли абонемент
        /// </summary>
        public bool IsActive => Status?.Trim().Equals("Активирован", StringComparison.InvariantCultureIgnoreCase) ?? false;

        /// <summary>
        /// Включена ли опция "Честная цена"
        /// </summary>
        public bool IsFairPriceOptionIncluded { get; set; }
    }
}
