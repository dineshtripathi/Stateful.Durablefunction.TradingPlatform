using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using TradingFunctionEntityDurable.EntityModel;

namespace TradingFunctionEntityDurable.EntityTrigger;
/// <summary>
/// The trading broker entity.
/// </summary>

public class TradingBrokerEntity
{
    /// <summary>
    /// Runs the trading broker entity.
    /// </summary>
    /// <param name="context">The context.</param>
    [FunctionName(nameof(RunTradingBrokerEntity))]
    public static void RunTradingBrokerEntity([EntityTrigger] IDurableEntityContext context)
    {
        try
        {
            Trade trade;
            var brokerState = context.GetState<TradingBrokerState>();
            switch (context.OperationName.ToLowerInvariant())
            {
                case "initiatetrade":
                    var tradeInfo = context.GetInput<TradeInfo>();
                    trade = new Trade
                    {
                        TradeId = context.EntityId.EntityKey,
                        StockSymbol = tradeInfo.StockSymbol,
                        Quantity = tradeInfo.Quantity,
                        Action = tradeInfo.Action,
                        Status = TradeStatus.Pending
                    };
                    context.SignalEntity(context.EntityId, nameof(TradingBrokerEntityDurableFunctions.RunTradeExecutionOrchestratorAsync), trade); // Start orchestrator
                    context.SetState(new TradingBrokerState { ActiveTrade = trade });
                    break;

                case "executetrade":
                    trade = context.GetInput<Trade>();
                    brokerState.ActiveTrade.Status = TradeStatus.Executed;
                    context.SetState(brokerState);
                    break;
                case "completetrade":
                    var remarks = context.GetInput<string>();
                    brokerState.IsDeleted = true;
                    brokerState.Remarks = remarks;
                    context.SetState(brokerState);
                    break;
            }
        }
        catch (Exception ex)
        {
            context.SetState(new TradingBrokerState());
            throw;
        }
    }
}