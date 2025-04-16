using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyWalletSystem.Infrastructure.Models
{
    public class CurrencyRate
    {
        public int Id { get; set; }
        public string Currency { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 6)")]
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }
    }
}
