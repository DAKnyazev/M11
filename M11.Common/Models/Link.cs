using M11.Common.Enums;

namespace M11.Common.Models
{
    /// <summary>
    /// Информация о ссылке
    /// </summary>
    public class Link
    {
        /// <summary>
        /// Тип ссылки
        /// </summary>
        public LinkType Type { get; set; }

        /// <summary>
        /// Относительный путь
        /// </summary>
        public string RelativeUrl { get; set; }
    }
}
