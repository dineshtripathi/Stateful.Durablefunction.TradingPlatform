using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TradingStatefulFunctionEntityDurable.EntityModel;
using TradingStatefulFunctionEntityDurable.EntityTrigger;

namespace TradingStatefulFunctionEntityDurable;

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
    /// <summary>
    /// Runs the finalize trade orchestrator async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A Task.</returns>
    [FunctionName(nameof(RunFinalizeTradeOrchestratorAsync))]
    public static async Task<string> RunFinalizeTradeOrchestratorAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        try
        {
            var tradeId = context.GetInput<string>();
            var entityId = new EntityId(nameof(TradingBrokerEntity.RunTradingBrokerEntity), tradeId);

            await context.CallEntityAsync(entityId, "FinalizeTrade");
            return $"Trade {tradeId} finalized successfully.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// Runs the soft delete trade orchestrator async.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>A Task.</returns>
    [FunctionName(nameof(RunSoftDeleteTradeOrchestratorAsync))]
    public static async Task<string> RunSoftDeleteTradeOrchestratorAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        try
        {
            var tradeId = context.GetInput<string>();
            var entityId = new EntityId(nameof(TradingBrokerEntity.RunTradingBrokerEntity), tradeId);

            await context.CallEntityAsync(entityId, "SoftDeleteTrade");
            return $"Trade {tradeId} soft deleted successfully.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}