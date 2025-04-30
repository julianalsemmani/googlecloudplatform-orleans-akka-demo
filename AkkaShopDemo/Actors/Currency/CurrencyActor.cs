using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using AkkaShopDemo.Utils;
using Shop;
using Akka.Actor;

namespace AkkaShopDemo.Actors.Currency
{
    public class CurrencyActor : ReceiveActor
    {
        private static readonly Dictionary<string, decimal> usdValues = new() {
            { "EUR", 1.0940m },
            { "USD", 1.0000m },
            { "JPY", 0.0067m },
            { "GBP", 1.3000m },
            { "TRY", 0.0270m },
            { "CAD", 0.7000m }
        };
        private readonly Guid primaryKey = Guid.NewGuid();
        private readonly ILogger<CurrencyActor> logger;
        
        public CurrencyActor(ILogger<CurrencyActor> logger)
        {
            this.logger = logger;
            
            Receive<CurrencyConversionRequest>(Convert);
            Receive<GetSupportedCurrenciesRequest>(GetSupportedCurrencies);
        }

        public void Convert(CurrencyConversionRequest request)
        {
            logger.LogInformation("Currency conversion requested with {Id}", primaryKey.ToString());
            logger.LogInformation("Converting {From} to {To}", request.From, request.ToCode);

            decimal fromValue = MoneyUtil.CombinePrice(request.From.Units, request.From.Nanos);
            decimal fromUsdValue = usdValues[request.From.CurrencyCode] * fromValue;
            decimal toUsdValue = usdValues[request.ToCode];
            decimal toValue = fromUsdValue / toUsdValue;

            Tuple<long, int> toUnitsNanos = MoneyUtil.SplitPrice(toValue);

            Sender.Tell(new Money {  
                CurrencyCode = request.ToCode,
                Units = toUnitsNanos.Item1,
                Nanos = toUnitsNanos.Item2
            });
        }

        public void GetSupportedCurrencies(GetSupportedCurrenciesRequest _)
        {
            logger.LogInformation("GetSupportedCurrencies activated with {Id}", primaryKey.ToString());
            logger.LogInformation("Getting supported currencies");

            Sender.Tell(new GetSupportedCurrenciesResponse { 
                CurrencyCodes = { "EUR", "USD", "JPY", "GBP", "TRY", "CAD" }
            });
        }
    }

    public class GetSupportedCurrenciesRequest 
    { }
}
