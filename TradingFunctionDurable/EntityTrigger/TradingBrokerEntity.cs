using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TradingStatefulFunctionEntityDurable.EntityModel;

namespace TradingStatefulFunctionEntityDurable.EntityTrigger;
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
    public static async Task RunTradingBrokerEntity([EntityTrigger] IDurableEntityContext context)
    {
        try
        {
            Trade trade;
            var brokerState = context.GetState<TradingBrokerState>() ?? new TradingBrokerState();
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
                    brokerState.ActiveTrade = trade;
                    context.SetState(brokerState);
                    break;

                case "executetrade":
                    trade = context.GetInput<Trade>();
                    brokerState.ActiveTrade.Status = TradeStatus.Executed;
                    context.SetState(brokerState);
                    break;

                case "completetrade":
                    var remarks = context.GetInput<string>();
                    brokerState.IsSoftDeleted = true;
                    brokerState.Remarks = remarks;
                    context.SetState(brokerState);
                    break;

                case "softdeletetrade":
                    brokerState.ActiveTrade.IsSoftDeleted = true;
                    context.SetState(brokerState);
                    break;

                case "softdeletestate":
                    brokerState.IsSoftDeleted = true;
                    context.SetState(brokerState);
                    break;

                case "finalizetrade":
                    brokerState.ActiveTrade.Status = TradeStatus.Completed;
                    context.SetState(brokerState);
                    break;

                default:
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