using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansShopDemo.Grains.Shipping.Interfaces;
using Shop;

namespace OrleansShopDemo.Grains.Shipping
{
    public class ShippingGrain : Grain, IShippingGrain
    {   
        private readonly ILogger<ShippingGrain> logger;
        public ShippingGrain(ILogger<ShippingGrain> logger)
        {
            this.logger = logger;
        }

        public Task<GetQuoteResponse> GetQuote(GetQuoteRequest request)
        {
            logger.LogInformation("Getting quote for {Items}", request.Items);

            return Task.FromResult(new GetQuoteResponse {
                CostUsd = new Money {
                    CurrencyCode = "USD",
                    Units = request.Items.Count * 2,
                    Nanos = 990000000
                }
            });
        }

        public Task<ShipOrderResponse> ShipOrder(ShipOrderRequest request)
        {
            logger.LogInformation("Shipping order requested with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Shipping order for {Address}", request.Address);

            var address = request.Address.StreetAddress + request.Address.City + request.Address.State;
            var bytes = MD5.HashData(Encoding.UTF8.GetBytes(address));
            var trackingId = Convert.ToHexStringLower(bytes);

            return Task.FromResult(new ShipOrderResponse {
                TrackingId = trackingId
            });
        }
    }
}
