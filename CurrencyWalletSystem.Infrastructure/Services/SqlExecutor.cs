using CurrencyWalletSystem.Infrastructure.Interfaces;

namespace CurrencyWalletSystem.Infrastructure.Services
{
    /// <summary>
    /// Provides a high-level abstraction for executing raw SQL commands using a lower-level raw SQL executor.
    /// </summary>
    public class SqlExecutor : ISqlExecutor
    {
        private readonly IRawSqlExecutor _rawSqlExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlExecutor"/> class.
        /// </summary>
        /// <param name="rawSqlExecutor">The raw SQL executor used to execute the SQL commands.</param>
        public SqlExecutor(IRawSqlExecutor rawSqlExecutor)
        {
            _rawSqlExecutor = rawSqlExecutor;
        }

        /// <inheritdoc/>
        public Task ExecuteAsync(string sql, object[] parameters)
        {
            return _rawSqlExecutor.ExecuteAsync(sql, parameters);
        }
    }
}
