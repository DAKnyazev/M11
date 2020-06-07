namespace M11.Common.Models
{
    /// <summary>
    /// Результат расчёта калькулятора поездки
    /// </summary>
    public class CalculatorResult
    {
        /// <summary>
        /// Цена по транспондеру
        /// </summary>
        public decimal TransponderCost { get; set; } 

        /// <summary>
        /// Цена за наличные или по стороннему транспондеру
        /// </summary>
        public decimal CashCost { get; set; }

        /// <summary>
        /// Составной ли маршрут?
        /// </summary>
        public bool IsComposite { get; set; }
    }
}
