using System;
using System.Threading.Tasks;
using Grpc.Core;
using Orleans;
using OrleansShopDemo.Grains.Checkout.Interfaces;
using OrleansShopDemo.Grains.Currency.Interfaces;
using Shop;
using static Shop.CheckoutService;

namespace OrleansShopDemo.Services 
{
    public class CheckoutService(IClusterClient clusterClient) : CheckoutServiceBase
    {
        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<ICheckoutGrain>(Guid.NewGuid()).PlaceOrder(request);
        }
    }
}
