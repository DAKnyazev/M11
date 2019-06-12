using System;
using System.Collections.Generic;
using System.Text;

namespace M11.Common
{
    public interface IDatabaseEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        string Id { get; set; }
    }
}
