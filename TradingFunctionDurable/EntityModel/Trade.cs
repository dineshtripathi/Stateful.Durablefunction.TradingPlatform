namespace TradingStatefulFunctionEntityDurable.EntityModel;
/// <summary>
/// The trade.
/// </summary>

public class Trade
{
    public string TradeId { get; set; }
    public string StockSymbol { get; set; }
    public int Quantity { get; set; }
    public TradeAction Action { get; set; }
    public TradeStatus Status { get; set; }
    public bool IsSoftDeleted { get; set; } = false;
}