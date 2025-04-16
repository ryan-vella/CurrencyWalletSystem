namespace CurrencyWalletSystem.Infrastructure.Strategies
{
    public interface IWalletBalanceStrategy
    {
        decimal Execute(decimal currentBalance, decimal amount);
    }
}
