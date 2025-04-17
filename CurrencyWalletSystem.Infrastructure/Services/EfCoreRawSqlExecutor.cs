using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace CurrencyWalletSystem.Infrastructure.Services
{
    [ExcludeFromCodeCoverage]
    public class EfCoreRawSqlExecutor : IRawSqlExecutor
    {
        private readonly AppDbContext _dbContext;

        public EfCoreRawSqlExecutor(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public Task<int> ExecuteAsync(string sql, object[] parameters)
        {
            return _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}
