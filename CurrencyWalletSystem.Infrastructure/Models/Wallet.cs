namespace CurrencyWalletSystem.Infrastructure.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "EUR";
    }
}
