using Grpc.Core;
using Orleans;
using OrleansShopDemo.Grains.Payment.Interfaces;
using Shop;
using System;
using System.Threading.Tasks;
using static Shop.PaymentService;

namespace OrleansShopDemo.Services
{
    public class PaymentService(IClusterClient clusterClient) : PaymentServiceBase
    {
        public override async Task<ChargeResponse> Charge(ChargeRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<IPaymentGrain>(Guid.NewGuid()).Charge(request);
        }
    }
}
