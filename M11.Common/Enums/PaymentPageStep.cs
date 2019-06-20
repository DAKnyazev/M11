namespace M11.Common.Enums
{
    /// <summary>
    /// Шаги переходов страницы оплаты
    /// </summary>
    public enum PaymentPageStep
    {
        None = 0,
        LoginPage = 1,
        MyPaymentPage = 2,
        SberbankCardPage = 3,
        ClientBankSmsPage = 4,
        ReturnFromClientBankPage = 5
    }
}
