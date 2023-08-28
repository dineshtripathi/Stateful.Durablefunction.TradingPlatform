namespace TradingFunctionEntityDurable.EntityModel;
public class TradingBrokerState
{
    public Trade ActiveTrade { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string Remarks { get; set; }
}
