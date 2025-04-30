using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hosting;
using AkkaShopDemo.Actors.Currency;
using Grpc.Core;
using Shop;
using static Shop.CurrencyService;

namespace AkkaShopDemo.Services 
{
    public class CurrencyService(IRequiredActor<CurrencyActor> currencyActor) : CurrencyServiceBase
    {
        public override async Task<GetSupportedCurrenciesResponse> GetSupportedCurrencies(Empty request, ServerCallContext context)
        {
            return await currencyActor.ActorRef.Ask<GetSupportedCurrenciesResponse>(new GetSupportedCurrenciesRequest());
        }

        public override async Task<Money> Convert(CurrencyConversionRequest request, ServerCallContext context)
        {
            return await currencyActor.ActorRef.Ask<Money>(request);
        }
    }
}
