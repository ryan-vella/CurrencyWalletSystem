namespace CurrencyWalletSystem.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines a contract for executing raw SQL queries that return an integer result,
    /// typically used for non-query commands like INSERT, UPDATE, or DELETE.
    /// </summary>
    public interface IRawSqlExecutor
    {
        /// <summary>
        /// Executes the specified raw SQL query asynchronously with the provided parameters.
        /// </summary>
        /// <param name="sql">The raw SQL command to execute.</param>
        /// <param name="parameters">The parameters to pass to the SQL command.</param>
        /// <returns>A task that represents the asynchronous operation, containing the number of affected rows.</returns>
        Task<int> ExecuteAsync(string sql, object[] parameters);
    }
}
