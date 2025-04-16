using CurrencyWalletSystem.Infrastructure.Enums;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyWalletSystem.Api.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new wallet with the specified currency.
        /// </summary>
        /// <param name="currency">The currency to associate with the wallet.</param>
        /// <returns>A response indicating the creation of the wallet.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromQuery] string currency)
        {
            _logger.LogInformation("Creating wallet with currency: {Currency}", currency);

            var wallet = await _walletService.CreateWalletAsync(currency);

            _logger.LogInformation("Wallet created with ID: {WalletId}", wallet.Id);

            return CreatedAtAction(nameof(GetWalletBalance), new { walletId = wallet.Id }, wallet);
        }

        /// <summary>
        /// Retrieves the balance of the specified wallet.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <param name="currency">Optional currency code. If not provided, the base currency will be used.</param>
        /// <returns>The balance of the wallet.</returns>
        [HttpGet("{walletId}")]
        public async Task<IActionResult> GetWalletBalance(int walletId, [FromQuery] string? currency = null)
        {
            _logger.LogInformation("Retrieving balance for wallet ID: {WalletId}, Currency: {Currency}", walletId, currency);

            var balance = await _walletService.GetWalletBalanceAsync(walletId, currency);

            if (string.IsNullOrEmpty(currency))
            {
                currency = await _walletService.GetBaseCurrencyAsync(walletId);
            }

            _logger.LogInformation("Wallet ID: {WalletId} has balance: {Balance} in currency: {Currency}", walletId, balance, currency);

            return Ok(new { WalletId = walletId, Balance = balance, Currency = currency });
        }

        /// <summary>
        /// Adjusts the balance of the specified wallet using the given strategy.
        /// </summary>
        /// <param name="walletId">The ID of the wallet to adjust.</param>
        /// <param name="amount">The amount to adjust the balance by.</param>
        /// <param name="currency">The currency to adjust the balance in.</param>
        /// <param name="strategy">The strategy to apply when adjusting the balance.</param>
        /// <returns>A no-content response indicating the adjustment is complete.</returns>
        [HttpPost("{walletId}/adjustbalance")]
        public async Task<IActionResult> AdjustBalance(
            int walletId,
            [FromQuery] decimal amount,
            [FromQuery] string currency,
            [FromQuery] string strategy)
        {
            if (!Enum.TryParse<WalletStrategyType>(strategy, true, out var strategyType))
            {
                _logger.LogWarning("Invalid strategy provided: {Strategy}", strategy);
                return BadRequest($"Invalid strategy: {strategy}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(WalletStrategyType)))}");
            }

            _logger.LogInformation("Adjusting balance for wallet ID: {WalletId}, Amount: {Amount}, Currency: {Currency}, Strategy: {Strategy}",
                walletId, amount, currency, strategy);

            await _walletService.AdjustWalletBalanceAsync(walletId, amount, currency, strategyType);

            _logger.LogInformation("Wallet ID: {WalletId} balance adjusted by {Amount} using {Strategy} strategy", walletId, amount, strategyType);

            return NoContent();
        }
    }
}