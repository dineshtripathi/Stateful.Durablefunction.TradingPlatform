using System.Diagnostics;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace TradingFunctionEntityDurableTester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new DurableClient(new DurableClientAttribute());

            var tradeId = Guid.NewGuid().ToString();

            var entityId = new EntityId(nameof(RunTradingBrokerEntity), tradeId);

            // 1. Initiate Trade
            await client.SignalEntityAsync(entityId, "InitiateTrade", new TradeInfo
            {
                StockSymbol = "AAPL",
                Quantity = 100,
                Action = "Buy"
            });

            // 2. Check Entity State
            var entityState = await client.ReadEntityStateAsync<TradingBrokerState>(entityId);
            Console.WriteLine($"Entity Status: {entityState.EntityExists} | {entityState.EntityState?.ActiveTrade?.Status}");

            // 3. Trigger Trade Execution
            await client.SignalEntityAsync(entityId, "ExecuteTrade", new Trade
            {
                TradeId = tradeId,
                StockSymbol = "AAPL",
                Quantity = 100,
                Action = "Buy"
            });

            // 4. Check Entity State Again
            entityState = await client.ReadEntityStateAsync<TradingBrokerState>(entityId);
            Console.WriteLine($"Entity Status: {entityState.EntityExists} | {entityState.EntityState?.ActiveTrade?.Status}");
        }
    }
}