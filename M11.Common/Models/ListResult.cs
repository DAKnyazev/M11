using System.Collections.Generic;

namespace M11.Common.Models
{
    /// <summary>
    /// Результат получения списка сущностей
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class ListResult<TEntity>
    {
        public ListResult()
        {
            List = new List<TEntity>();
        }
        /// <summary>
        /// Была ли ошибка в процессе получения списка
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Список сущностей
        /// </summary>
        public List<TEntity> List { get; }

        /// <summary>
        /// Добавление сущности
        /// </summary>
        public void Add(TEntity entity)
        {
            List.Add(entity);
        }
    }
}
