using System;
using System.Collections.Generic;
using System.Net;
using M11.Common.Enums;
using RestSharp;

namespace M11.Common.Models
{
    /// <summary>
    /// Информация о договоре
    /// </summary>
    public class Info
    {
        public Info()
        {
            Tickets = new List<Ticket>();
        }

        /// <summary>
        /// Дата запроса информации (для опеределения свежести данных)
        /// </summary>
        public DateTime RequestDate { get; set; }

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

        /// <summary>
        /// Ссылки на основные страницы
        /// </summary>
        public List<Link<LinkType>> Links { get; set; }

        /// <summary>
        /// Куки, которые нужно использовать при запросах без логина/пароля
        /// </summary>
        public CookieContainer CookieContainer { get; set; }
    }
}
