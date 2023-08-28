using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TradingStatefulFunctionEntityDurable;
using TradingStatefulFunctionEntityDurable.Processor;
[assembly: FunctionsStartup(typeof(Startup))]
namespace TradingStatefulFunctionEntityDurable;

public class Startup : FunctionsStartup
{
    /// <summary>
    /// Configures the.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddTransient<TradeManager>();
    }
}