using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace M11.Common.Models.BillSummary
{
    /// <summary>
    /// Параметр получения информации по счету
    /// </summary>
    public class BillsSummaryParam
    {
        public BillsSummaryParam(DateTime start, DateTime end)
        {
            F = "mdate";
            E = true;
            O = "between";
            V = new List<BillsSummaryDateParam>
            {
                new BillsSummaryDateParam
                {
                    Date = start
                },
                new BillsSummaryDateParam
                {
                    Date = end
                }
            };
        }

        [JsonProperty("f")]
        public string F { get; set; }

        [JsonProperty("e")]
        public bool E { get; set; }

        [JsonProperty("o")]
        public string O { get; set; }

        [JsonProperty("v")]
        public List<BillsSummaryDateParam> V { get; set; }
    }
}
