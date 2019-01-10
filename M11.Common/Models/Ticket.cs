using System;
using System.Text.RegularExpressions;

namespace M11.Common.Models
{
    /// <summary>
    /// Информация об абонементе
    /// </summary>
    public class Ticket
    {
        private string _description = string.Empty;

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
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                DescriptionParts = _description?.Split(',');
                TotalTripsCount = DescriptionParts?.Length > 0
                    ? Regex.Match(DescriptionParts[0], @"\d+").Value
                    : string.Empty;
            }
        }

        /// <summary>
        /// Разбитое по запятым описание абонемента
        /// </summary>
        public string[] DescriptionParts { get; private set; } = new string[0];

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
        /// Общее количество поездок
        /// </summary>
        public string TotalTripsCount { get; private set; }

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
