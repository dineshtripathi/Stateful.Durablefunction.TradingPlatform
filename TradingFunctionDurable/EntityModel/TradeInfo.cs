namespace TradingFunctionEntityDurable.EntityModel;

public class TradeInfo
{
    public string StockSymbol { get; set; }
    public int Quantity { get; set; }
    public TradeAction Action { get; set; }
}