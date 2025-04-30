using System;
using System.Threading.Tasks;
using Grpc.Core;
using Orleans;
using OrleansShopDemo.Grains.Shipping.Interfaces;
using Shop;
using static Shop.ShippingService;

namespace OrleansShopDemo.Services 
{
    public class ShippingService(IClusterClient clusterClient) : ShippingServiceBase
    {
        public override async Task<GetQuoteResponse> GetQuote(GetQuoteRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<IShippingGrain>(Guid.NewGuid()).GetQuote(request);
        }

        public override async Task<ShipOrderResponse> ShipOrder(ShipOrderRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<IShippingGrain>(Guid.NewGuid()).ShipOrder(request);
        }
    }
}
