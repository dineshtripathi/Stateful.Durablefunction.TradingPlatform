using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TradingFunctionEntityDurable.Processor;

namespace TradingFunctionEntityDurable;

/// <summary>
/// The startup.
/// </summary>
public class Startup : FunctionsStartup
{
    /// <summary>
    /// Configures the.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<TradeManager>();
    }
}