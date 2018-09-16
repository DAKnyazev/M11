using System;
using Newtonsoft.Json;

namespace M11.Common.Models.BillSummary
{
    /// <summary>
    /// Параметр даты
    /// </summary>
    public class BillsSummaryDateParam
    {
        [JsonIgnore]
        public DateTime Date { get; set; }

        [JsonProperty("fltfield")]
        public string FltField => Date.ToString("yyyy-MM-dd HH:mm:ss");

        [JsonProperty("__changed_fields")]
        public string ChangedFields => "";
    }
}
