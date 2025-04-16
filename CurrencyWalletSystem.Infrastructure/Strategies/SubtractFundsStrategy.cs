namespace CurrencyWalletSystem.Infrastructure.Strategies
{
    public class SubtractFundsStrategy : IWalletBalanceStrategy
    {
        public decimal Execute(decimal currentBalance, decimal amount)
        {
            if (currentBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            return currentBalance - amount;
        }
    }
}
