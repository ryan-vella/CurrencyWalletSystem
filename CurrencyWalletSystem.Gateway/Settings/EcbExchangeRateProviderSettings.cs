namespace CurrencyWalletSystem.Gateway.Settings
{
    /// <summary>
    /// Represents the settings for the ECB exchange rate provider.
    /// </summary>
    public class EcbExchangeRateProviderSettings
    {
        /// <summary>
        /// Gets or sets the URL for fetching exchange rates from the ECB.
        /// </summary>
        public string Url { get; set; }
    }
}