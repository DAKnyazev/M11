using System;
using System.Collections.Generic;

namespace M11.Common.Models
{
    /// <summary>
    /// Информация о договоре
    /// </summary>
    public class Info
    {
        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Активен ли договор
        /// </summary>
        public bool IsActive => Status?.Trim().Equals("Активен", StringComparison.InvariantCultureIgnoreCase) ?? false;

        /// <summary>
        /// Баланс в рублях
        /// </summary>
        public string Balance { get; set; }

        /// <summary>
        /// Абонементы
        /// </summary>
        public List<Ticket> Tickets { get; set; }
    }
}
