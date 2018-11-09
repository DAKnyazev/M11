namespace M11.Common.Models.BillSummary
{
    public class Bill : BaseBill
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ПВП Въезда
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// ПВП выезда
        /// </summary>
        public string ExitPoint { get; set; }

        /// <summary>
        /// Комментарий, в каком пункте мы въехали или выехали, если он был за пределами участка 15-58 км М-11
        /// </summary>
        public string ForeigtPointComment { get; set; }

        /// <summary>
        /// PAN
        /// </summary>
        public string PAN { get; set; }

        /// <summary>
        /// Класс ТС
        /// </summary>
        public string CarClass { get; set; }
    }
}
