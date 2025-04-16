namespace CurrencyWalletSystem.Infrastructure.Strategies
{
    public class AddFundsStrategy : IWalletBalanceStrategy
    {
        public decimal Execute(decimal currentBalance, decimal amount)
        {
            return currentBalance + amount;
        }
    }
}