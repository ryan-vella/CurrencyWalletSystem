namespace CurrencyWalletSystem.Infrastructure.Strategies
{
    public class ForceSubtractFundsStrategy : IWalletBalanceStrategy
    {
        public decimal Execute(decimal currentBalance, decimal amount)
        {
            return currentBalance - amount;
        }
    }
}