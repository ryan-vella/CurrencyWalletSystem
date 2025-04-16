using CurrencyWalletSystem.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyWalletSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    }
}
