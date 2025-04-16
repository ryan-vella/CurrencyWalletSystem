namespace CurrencyWalletSystem.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="logger">Logger to log messages for the worker.</param>
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes the background worker service.
    /// Logs the current time at intervals, and continues until cancellation is requested.
    /// </summary>
    /// <param name="stoppingToken">Token that signals when the background service should stop.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogWorkerStatus();
            await DelayExecution(stoppingToken);
        }
    }

    /// <summary>
    /// Logs the current worker status, including the time the worker is running.
    /// </summary>
    private void LogWorkerStatus()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
    }

    /// <summary>
    /// Delays the execution of the worker for a specified period.
    /// </summary>
    /// <param name="stoppingToken">Token that can be used to cancel the delay.</param>
    /// <returns>A Task representing the delay operation.</returns>
    private async Task DelayExecution(CancellationToken stoppingToken)
    {
        await Task.Delay(1000, stoppingToken);
    }
}
