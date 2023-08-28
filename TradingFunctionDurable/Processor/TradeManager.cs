using System;
using TradingStatefulFunctionEntityDurable.EntityModel;

namespace TradingStatefulFunctionEntityDurable.Processor;

/// <summary>
/// The trade manager.
/// </summary>
public class TradeManager
{
    /// <summary>
    /// Completes the trade.
    /// </summary>
    /// <param name="trade">The trade.</param>
    /// <returns>A TradeCompletionInfo.</returns>
    public TradeCompletionInfo CompleteTrade(Trade trade)
    {
        // This could involve interacting with other systems, APIs, databases, etc.

        bool isTradeSuccessful = true; // as of now setting up as true , in real world scenario it will be set by other systems,api's ,database, etc 

        TradeCompletionInfo completionInfo = new TradeCompletionInfo();

        completionInfo.CompletionDate = DateTime.UtcNow;
        completionInfo.CompletedBy = "AutomatedSystem"; 

        if (isTradeSuccessful)
        {
            completionInfo.CompletionStatus = "Successful";
            completionInfo.FinalTradeValue = trade.Quantity * 50; 
            completionInfo.Fees = completionInfo.FinalTradeValue * 0.02m; 
            completionInfo.Tax = completionInfo.FinalTradeValue * 0.05m; 
            completionInfo.Remarks = "Trade completed successfully.";
        }
        else // will not be executed as of now
        {
            completionInfo.CompletionStatus = "Failed";
            completionInfo.ReasonForFailure = "Insufficient funds."; 
            completionInfo.Remarks = "Trade could not be completed due to insufficient funds.";
        }

        return completionInfo;
    }

}