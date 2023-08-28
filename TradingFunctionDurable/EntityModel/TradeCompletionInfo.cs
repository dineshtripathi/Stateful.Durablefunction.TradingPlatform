using System;

namespace TradingStatefulFunctionEntityDurable.EntityModel;
/// <summary>
/// The trade completion info.
/// </summary>

public class TradeCompletionInfo
{
    public string Remarks { get; set; }
    public DateTime CompletionDate { get; set; }
    public string CompletedBy { get; set; }
    public string CompletionStatus { get; set; } 
    public string ReasonForFailure { get; set; }
    public decimal FinalTradeValue { get; set; }
    public decimal Fees { get; set; }
    public decimal Tax { get; set; }
}