namespace CurrencyWalletSystem.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines a contract for executing raw SQL commands asynchronously.
    /// Intended for scenarios where the result of the execution is not required.
    /// </summary>
    public interface ISqlExecutor
    {
        /// <summary>
        /// Executes the specified SQL command asynchronously with the provided parameters.
        /// </summary>
        /// <param name="sql">The raw SQL command to execute.</param>
        /// <param name="parameters">The parameters to include with the SQL command.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(string sql, object[] parameters);
    }
}
