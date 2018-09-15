using M11.Common.Enums;

namespace M11.Common.Models
{
    /// <summary>
    /// Информация о ссылке
    /// </summary>
    public class Link<T> where T : System.Enum
    {
        /// <summary>
        /// Тип ссылки
        /// </summary>
        public T Type { get; set; }

        /// <summary>
        /// Относительный путь
        /// </summary>
        public string RelativeUrl { get; set; }
    }
}
