using System;

namespace M11.Common.Models
{
    /// <summary>
    /// Базовая информация
    /// </summary>
    public abstract class BaseInfo
    {
        /// <summary>
        /// Дата запроса информации (для опеределения свежести данных)
        /// </summary>
        public DateTime RequestDate { get; set; }
    }
}
