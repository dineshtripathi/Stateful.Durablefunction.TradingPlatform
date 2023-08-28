using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using TradingStatefulFunctionEntityDurable.EntityModel;
using TradingStatefulFunctionEntityDurable.EntityTrigger;

namespace TradingStatefulFunctionEntityDurable.ActivityTrigger;
/// <summary>
/// The trade execution activities.
/// </summary>

public class TradeExecutionActivities
{
    private readonly ILogger<TradeExecutionActivities> _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeExecutionActivities"/> class.
    /// </summary>
    /// <param name="log">The log.</param>
    public TradeExecutionActivities(ILogger<TradeExecutionActivities> log)
    {
        _log = log;
    }

    /// <summary>
    /// Runs the execute trade async.
    /// </summary>
    /// <param name="trade">The trade.</param>
    /// <param name="log">The log.</param>
    /// <returns>A Task.</returns>
    [FunctionName(nameof(RunExecuteTradeAsync))]
    public static async Task<string> RunExecuteTradeAsync([ActivityTrigger] Trade trade, ILogger log)
    {
        try
        {
            await Task.Delay(100); // Simulating asynchronous processing
            log.LogInformation(
                $"Executing trade: {trade.TradeId}, {trade.Action} {trade.Quantity} shares of {trade.StockSymbol}");
            return $"Trade {trade.TradeId} executed.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// Waits the for trade execution completion async.
    /// </summary>
    /// <param name="tradeId">The trade id.</param>
    /// <param name="client">The client.</param>
    /// <param name="log">The log.</param>
    /// <returns>A Task.</returns>
    [FunctionName(nameof(WaitForTradeExecutionCompletionAsync))]
    public static async Task WaitForTradeExecutionCompletionAsync(
        [ActivityTrigger] string tradeId,
        [DurableClient] IDurableClient client,
        ILogger log)
    {
        try
        {
            const int retryIntervalSeconds = 10;
            const int maxRetries = 10;

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                var entityState =
                    await client.ReadEntityStateAsync<TradingBrokerState>(
                        new EntityId(nameof(TradingBrokerEntity.RunTradingBrokerEntity), tradeId));
                if (entityState.EntityExists && entityState.EntityState.ActiveTrade.Status == TradeStatus.Executed)
                {
                    return;
                }

                var nextCheck = DateTime.UtcNow.AddSeconds(retryIntervalSeconds);
                log.LogInformation(
                    $"Trade execution not completed yet. Retrying in {retryIntervalSeconds} seconds...");
                await Task.Delay(nextCheck - DateTime.UtcNow);
            }

            log.LogWarning("Trade execution did not complete within the expected time.");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error while waiting for trade execution completion.");
            throw;
        }
    }
}