using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansShopDemo.Grains.Currency.Interfaces;
using OrleansShopDemo.Utils;
using Shop;

namespace OrleansShopDemo.Grains.Currency
{
    public class CurrencyGrain : Grain, ICurrencyGrain
    {
        private static readonly Dictionary<string, decimal> usdValues = new() {
            { "EUR", 1.0940m },
            { "USD", 1.0000m },
            { "JPY", 0.0067m },
            { "GBP", 1.3000m },
            { "TRY", 0.0270m },
            { "CAD", 0.7000m }
        };
        private readonly ILogger<CurrencyGrain> logger;
        
        public CurrencyGrain(ILogger<CurrencyGrain> logger)
        {
            this.logger = logger;
        }

        public Task<Money> Convert(CurrencyConversionRequest request)
        {
            logger.LogInformation("Currency conversion requested with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Converting {From} to {To}", request.From, request.ToCode);

            decimal fromValue = MoneyUtil.CombinePrice(request.From.Units, request.From.Nanos);
            decimal fromUsdValue = usdValues[request.From.CurrencyCode] * fromValue;
            decimal toUsdValue = usdValues[request.ToCode];
            decimal toValue = fromUsdValue / toUsdValue;

            Tuple<long, int> toUnitsNanos = MoneyUtil.SplitPrice(toValue);

            return Task.FromResult(new Money {  
                CurrencyCode = request.ToCode,
                Units = toUnitsNanos.Item1,
                Nanos = toUnitsNanos.Item2
            });
        }

        public Task<GetSupportedCurrenciesResponse> GetSupportedCurrencies()
        {
            logger.LogInformation("GetSupportedCurrencies activated with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Getting supported currencies");

            return Task.FromResult(new GetSupportedCurrenciesResponse { 
                CurrencyCodes = { "EUR", "USD", "JPY", "GBP", "TRY", "CAD" }
            });
        }
    }
}
