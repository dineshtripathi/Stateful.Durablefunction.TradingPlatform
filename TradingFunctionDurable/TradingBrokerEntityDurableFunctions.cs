using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;
using TradingFunctionEntityDurable.EntityModel;
using TradingFunctionEntityDurable.EntityTrigger;

namespace TradingFunctionEntityDurable
{
    /// <summary>
    /// The trading broker entity durable functions.
    /// </summary>
    public static class TradingBrokerEntityDurableFunctions
    {
        /// <summary>
        /// Runs the trade execution orchestrator async.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A Task.</returns>
        [FunctionName(nameof(RunTradeExecutionOrchestratorAsync))]
        public static async Task<string> RunTradeExecutionOrchestratorAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                var trade = context.GetInput<Trade>();
                var entityId = new EntityId(nameof(TradingBrokerEntity.RunTradingBrokerEntity), trade.TradeId);

                await context.CallEntityAsync(entityId, "ExecuteTrade", trade);
                var timeout = TimeSpan.FromMinutes(10);

                var tradeResult = await context.WaitForExternalEvent<string>(trade.TradeId, timeout);

                return tradeResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}