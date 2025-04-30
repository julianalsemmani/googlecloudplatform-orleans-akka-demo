using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hosting;
using AkkaShopDemo.Actors.Shipping;
using Grpc.Core;
using Shop;
using static Shop.ShippingService;

namespace AkkaShopDemo.Services 
{
    public class ShippingService(IRequiredActor<ShippingActor> shippingActor) : ShippingServiceBase
    {
        public override async Task<GetQuoteResponse> GetQuote(GetQuoteRequest request, ServerCallContext context)
        {
            return await shippingActor.ActorRef.Ask<GetQuoteResponse>(request);
        }

        public override async Task<ShipOrderResponse> ShipOrder(ShipOrderRequest request, ServerCallContext context)
        {
            return await shippingActor.ActorRef.Ask<ShipOrderResponse>(request);
        }
    }
}
