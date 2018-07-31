using System.ComponentModel;

namespace M11.Common.Enums
{
    /// <summary>
    /// Типы ссылок
    /// </summary>
    public enum LinkType
    {
        /// <summary>
        /// Не установлено
        /// </summary>
        [Description("Не установлено")]
        NotSet = 0,
        /// <summary>
        /// Общее
        /// </summary>
        [Description("Общее")]
        Common = 1,
        /// <summary>
        /// Лицевой счет
        /// </summary>
        [Description("Лицевой счет")]
        Account = 2,
        /// <summary>
        /// Пополнить счет
        /// </summary>
        [Description("Пополнить счет")]
        Payment = 3,
        /// <summary>
        /// Заявки и обращения
        /// </summary>
        [Description("Заявки и обращения")]
        Requests = 4,
        /// <summary>
        /// Выход
        /// </summary>
        [Description("Выход")]
        Exit = 5
    }
}
