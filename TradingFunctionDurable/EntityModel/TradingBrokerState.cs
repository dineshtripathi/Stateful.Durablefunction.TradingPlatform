namespace TradingStatefulFunctionEntityDurable.EntityModel;
/// <summary>
/// The trading broker state.
/// </summary>

public class TradingBrokerState
{
    public Trade ActiveTrade { get; set; }
    public bool IsSoftDeleted { get; set; } = false;
    public string Remarks { get; set; }
}
