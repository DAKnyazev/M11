using System;
using System.Collections.Generic;

namespace M11.Services
{
    public abstract class BaseInfoService
    {
        protected const string ParentIdParamName = "_parent_id";
        protected const string IlinkIdParamName = "__ilink_id__=";
        protected const string SaveMonthlyBillsFilter = "/onyma/rm/party/bills_summary2/inline-filter/save/";
        protected static readonly Dictionary<string, string> RowIdEncodeDictionary = new Dictionary<string, string>
        {
            { "$", "$00" },
            { "/", "$01" },
            { "+", "$02" },
            { "&", "$03" },
            { ",", "$04" },
            { ":", "$05" },
            { ";", "$06" },
            { "=", "$07" },
            { "?", "$08" },
            { "@", "$09" }
        };

        /// <summary>
        /// Получение содержимого тега
        /// </summary>
        protected static string GetTagValue(string content, string startingTag, string endingTag, int index = 1, bool isAttributesIncluded = true)
        {
            var startIndex = 0;
            for (int i = 0; i < index; i++)
            {
                if (i != 0)
                {
                    content = content.Substring(startIndex + 1);
                }

                startIndex = content.IndexOf(startingTag, StringComparison.InvariantCultureIgnoreCase);
            }

            if (!isAttributesIncluded && !startingTag.EndsWith(">"))
            {
                startIndex = content.IndexOf('>', startIndex);
            }

            if (startIndex > 0)
            {
                var endIndex = content.IndexOf(endingTag, startIndex, StringComparison.InvariantCultureIgnoreCase);

                return content.Substring(startIndex, endIndex - startIndex + endingTag.Length);
            }

            return string.Empty;
        }

        /// <summary>
        /// Получение значения параметра из пути
        /// </summary>
        protected static string GetParamValue(string path, string paramName)
        {
            var startIndex = path.IndexOf(paramName, StringComparison.InvariantCultureIgnoreCase);
            var tmp = path.Substring(startIndex + paramName.Length);
            var endIndex = tmp.IndexOf("&", StringComparison.InvariantCulture);

            return endIndex < 0 ? tmp : tmp.Substring(0, endIndex);
        }

        /// <summary>
        /// Получение значения аттрибута
        /// </summary>
        protected static string GetAttributeValue(string content, string attributeName)
        {
            var startIndex = content.IndexOf(attributeName, StringComparison.InvariantCultureIgnoreCase);
            var tmp = content.Substring(startIndex + attributeName.Length);
            var endIndex = tmp.IndexOf("\"", StringComparison.InvariantCulture);

            return tmp.Substring(0, endIndex);
        }

        /// <summary>
        /// Закодировать идентификатор строки аккаунта
        /// </summary>
        protected static string EncodeRowId(string rowId)
        {
            foreach (var item in RowIdEncodeDictionary)
            {
                rowId = rowId.Replace(item.Key, item.Value);
            }

            return rowId;
        }
    }
}
