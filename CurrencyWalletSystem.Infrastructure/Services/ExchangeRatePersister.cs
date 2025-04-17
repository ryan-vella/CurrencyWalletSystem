using CurrencyWalletSystem.Gateway.Models;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace CurrencyWalletSystem.Infrastructure.Services
{
    public class ExchangeRatePersister : IExchangeRatePersister
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly ILogger<ExchangeRatePersister> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeRatePersister"/> class.
        /// </summary>
        /// <param name="sqlExecutor">The SQL executor used to persist exchange rate data to the database.</param>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        public ExchangeRatePersister(ISqlExecutor sqlExecutor, ILogger<ExchangeRatePersister> logger)
        {
            _sqlExecutor = sqlExecutor;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task PersistAsync(IEnumerable<ExchangeRate> rates)
        {
            if (!rates.Any())
            {
                _logger.LogWarning("No exchange rates to persist.");
                return;
            }

            _logger.LogInformation("Persisting {Count} exchange rates...", rates.Count());

            var finalSql = BuildMergeSql(rates, out object[] parameters);

            try
            {
                await _sqlExecutor.ExecuteAsync(finalSql, parameters);
                _logger.LogInformation("Exchange rates persisted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while persisting exchange rates.");
                throw;
            }
        }

        // <summary>
        /// Builds a parameterized MERGE SQL command to insert or update currency rates.
        /// </summary>
        /// <param name="rates">The collection of exchange rates.</param>
        /// <param name="parameters">The output parameters for the SQL command.</param>
        /// <returns>The final SQL command string.</returns>
        private string BuildMergeSql(IEnumerable<ExchangeRate> rates, out object[] parameters)
        {
            const string baseSql = @"
                MERGE INTO CurrencyRates AS Target
                USING (VALUES
                    {0}
                ) AS Source (Currency, Rate, Date)
                ON Target.Currency = Source.Currency AND Target.Date = Source.Date
                WHEN MATCHED THEN
                    UPDATE SET Rate = Source.Rate
                WHEN NOT MATCHED THEN
                    INSERT (Currency, Rate, Date) VALUES (Source.Currency, Source.Rate, Source.Date);";

            var rows = string.Join(",\n", rates.Select((r, i) =>
                $"(@p{i * 3}, @p{i * 3 + 1}, @p{i * 3 + 2})"));

            parameters = rates
                .SelectMany(r => new object[] { r.Currency, r.Rate, r.Date })
                .ToArray();

            return string.Format(baseSql, rows);
        }
    }
}
