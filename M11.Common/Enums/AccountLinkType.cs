using System.ComponentModel;

namespace M11.Common.Enums
{
    public enum AccountLinkType
    {
        /// <summary>
        /// Не установлено
        /// </summary>
        [Description("Не установлено")]
        NotSet = 0,

        /// <summary>
        /// Договор
        /// </summary>
        [Description("Договор")]
        Contract = 1,

        /// <summary>
        /// Транспондеры / Карты
        /// </summary>
        [Description("Транспондеры / Карты")]
        Transponders = 2,

        /// <summary>
        /// Лицевой счет
        /// </summary>
        [Description("Лицевой счет")]
        Account = 3,

        /// <summary>
        /// Документы
        /// </summary>
        [Description("Документы")]
        Documents = 4
    }
}
