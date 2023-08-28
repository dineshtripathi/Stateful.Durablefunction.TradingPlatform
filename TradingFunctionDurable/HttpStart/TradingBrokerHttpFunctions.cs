using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TradingStatefulFunctionEntityDurable.EntityModel;
using TradingStatefulFunctionEntityDurable.EntityTrigger;
using TradingStatefulFunctionEntityDurable.Processor;

namespace TradingStatefulFunctionEntityDurable.HttpStart;

/// <summary>
/// The trading broker http functions.
/// </summary>
public class TradingBrokerHttpFunctions
{
    private readonly ILogger<TradingBrokerHttpFunctions> _log;
    private readonly TradeManager _tradeManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TradingBrokerHttpFunctions"/> class.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="tradeManager"></param>
    public TradingBrokerHttpFunctions(ILogger<TradingBrokerHttpFunctions> log, TradeManager tradeManager)
    {
        _log = log;
        _tradeManager = tradeManager;
    }

    /// <summary>
    /// Runs the trading broker http start.
    /// </summary>
    /// <param name="req">The req.</param>
    /// <param name="client">The client.</param>
    /// <param name="entityKey">The entity key.</param>
    /// <param name="operationType"></param>
    /// <param name="log"></param>
    /// <returns>A Task.</returns>
    [FunctionName("TradingBrokerHttpStart")]
    public async Task<HttpResponseMessage> RunTradingBrokerHttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TradingBroker/{entityKey}/{operationType}")] HttpRequestMessage req,
        [DurableClient] IDurableEntityClient client,
        string entityKey,
        string operationType,
        ILogger log)
    {
        try
        {
            var entityId = new EntityId(nameof(TradingBrokerEntity.RunTradingBrokerEntity), entityKey);
            var tradeInfo = await req.Content.ReadAsAsync<TradeInfo>();

            switch (operationType.ToLower())
            {
                case "initiatetrade":
                    await client.SignalEntityAsync(entityId, "initiateTrade", tradeInfo);
                    break;

                case "executetrade":
                    await client.SignalEntityAsync(entityId, "executeTrade", tradeInfo);
                    break;
                case "completetrade":
                    var remarks = await req.Content.ReadAsStringAsync();
                    await client.SignalEntityAsync(entityId, "completetrade", remarks);
                    break;
                case "finalizetrade":
                    await client.SignalEntityAsync(entityId, "finalizetrade", null); // Assuming no additional data is needed
                    break;
                default:
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent($"Invalid operation type: {operationType}.", Encoding.UTF8, "application/json")
                    };
            }

            var checkState = await client.ReadEntityStateAsync<TradingBrokerState>(entityId);
            if (!checkState.EntityExists)
            {
                log.LogWarning($"Entity {entityId.EntityKey} was not created after signaling.");
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{operationType} operation completed.", Encoding.UTF8, "application/json")
            };
        }
        catch (Exception ex)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ex.Message, Encoding.UTF8, "application/json")
            };
        }
    }

    /// <summary>
    /// Runs the get broker state.
    /// </summary>
    /// <param name="req">The req.</param>
    /// <param name="client">The client.</param>
    /// <param name="entityKey">The entity key.</param>
    /// <param name="operationType">The operation type.</param>
    /// <param name="log">The log.</param>
    /// <returns>A Task.</returns>
    [FunctionName("GetBrokerState")]
    public async Task<HttpResponseMessage> RunGetBrokerState(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "TradingBroker/{entityKey}/{operationType}")] HttpRequestMessage req,
    [DurableClient] IDurableEntityClient client,
    string entityKey,
    string operationType, ILogger log)
    {
        try
        {
            var entityId = new EntityId(nameof(TradingBrokerEntity.RunTradingBrokerEntity), entityKey);
            EntityStateResponse<TradingBrokerState> stateResponse = await client.ReadEntityStateAsync<TradingBrokerState>(entityId);

            if (!stateResponse.EntityExists)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("Trading broker entity not found.")
                };
            }

            var brokerState = stateResponse.EntityState;

            switch (operationType.ToLower())
            {
                case "initiatetrade":
                    if (brokerState.ActiveTrade.Status != TradeStatus.Pending)
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = new StringContent("Trade has already been processed or executed.", Encoding.UTF8, "application/json")
                        };
                    }
                    break;

                case "executetrade":
                    if (brokerState.ActiveTrade.Status != TradeStatus.Executed)
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = new StringContent("Trade has not been executed yet.", Encoding.UTF8, "application/json")
                        };
                    }
                    break;
               
                default:
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent($"Invalid operation type: {operationType}.", Encoding.UTF8, "application/json")
                    };
            }
           return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(brokerState), Encoding.UTF8, "application/json")
            };
        }
        catch (Exception ex)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ex.Message, Encoding.UTF8, "application/json")
            };
        }
    }
    /// <summary>
    /// Signals the trade completion.
    /// </summary>
    /// <param name="req">The req.</param>
    /// <param name="client">The client.</param>
    /// <param name="entityKey">The entity key.</param>
    /// <returns>A Task.</returns>
    [FunctionName("SignalTradeCompletion")]
    public async Task<HttpResponseMessage> SignalTradeCompletion(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TradingBroker/{entityKey}/complete")] HttpRequestMessage req,
        [DurableClient] IDurableEntityClient client,
        string entityKey)
    {
        try
        {
            var remarks = await req.Content.ReadAsStringAsync();
            var entityId = new EntityId(nameof(TradingBrokerEntity.RunTradingBrokerEntity), entityKey);
            await client.SignalEntityAsync(entityId, "completetrade", remarks);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Trade marked as completed.", Encoding.UTF8, "application/json")
            };
        }
        catch (Exception ex)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ex.Message, Encoding.UTF8, "application/json")
            };
        }
    }

    /// <summary>
    /// Finalizes the trade completion.
    /// </summary>
    /// <param name="req">The req.</param>
    /// <param name="client"></param>
    /// <param name="tradeId">The trade id.</param>
    /// <returns>A Task.</returns>
    [FunctionName("FinalizeTradeFunction")]
    public async Task<HttpResponseMessage> FinalizeTradeFunction(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "FinalizeTrade/{tradeId}")] HttpRequestMessage req,
        [DurableClient] IDurableEntityClient client,
        string tradeId)
    {
        try
        {
            var entityId = new EntityId(nameof(TradingBrokerEntity.RunTradingBrokerEntity), tradeId);
            var stateResponse = await client.ReadEntityStateAsync<TradingBrokerState>(entityId);

            if (!stateResponse.EntityExists)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent($"Trade with ID {tradeId} not found.", Encoding.UTF8, "application/json")
                };
            }

            var trade = stateResponse.EntityState.ActiveTrade;

            var completionInfo = _tradeManager.CompleteTrade(trade);

            // Save the completionInfo to a database or other persistent storage if needed

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(completionInfo), Encoding.UTF8, "application/json")
            };
        }
        catch (Exception ex)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ex.Message, Encoding.UTF8, "application/json")
            };
        }
    }
    [FunctionName("SoftDeleteTradeHttpEndpoint")]
    public async Task<HttpResponseMessage> SoftDeleteTrade(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TradingBroker/{entityKey}/softdeletetrade")] HttpRequestMessage req,
        [DurableClient] IDurableEntityClient client,
        string entityKey)
    {
        var entityId = new EntityId(nameof(TradingBrokerEntity), entityKey);
        await client.SignalEntityAsync(entityId, "softdeletetrade", null);
        return new HttpResponseMessage(HttpStatusCode.OK);
    }

    [FunctionName("SoftDeleteStateHttpEndpoint")]
    public async Task<HttpResponseMessage> SoftDeleteState(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TradingBroker/{entityKey}/softdeletestate")] HttpRequestMessage req,
        [DurableClient] IDurableEntityClient client,
        string entityKey)
    {
        var entityId = new EntityId(nameof(TradingBrokerEntity), entityKey);
        await client.SignalEntityAsync(entityId, "softdeletestate", null);
        return new HttpResponseMessage(HttpStatusCode.OK);
    }
}