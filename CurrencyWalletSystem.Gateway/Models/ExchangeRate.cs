namespace CurrencyWalletSystem.Gateway.Models
{
    public class ExchangeRate
    {
        public string Currency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }
    }
}