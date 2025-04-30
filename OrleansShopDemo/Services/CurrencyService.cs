using System;
using System.Threading.Tasks;
using Grpc.Core;
using Orleans;
using OrleansShopDemo.Grains.Currency.Interfaces;
using Shop;
using static Shop.CurrencyService;

namespace OrleansShopDemo.Services 
{
    public class CurrencyService(IClusterClient clusterClient) : CurrencyServiceBase
    {
        public override async Task<GetSupportedCurrenciesResponse> GetSupportedCurrencies(Empty request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<ICurrencyGrain>(Guid.NewGuid()).GetSupportedCurrencies();
        }

        public override async Task<Money> Convert(CurrencyConversionRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<ICurrencyGrain>(Guid.NewGuid()).Convert(request);
        }
    }
}
