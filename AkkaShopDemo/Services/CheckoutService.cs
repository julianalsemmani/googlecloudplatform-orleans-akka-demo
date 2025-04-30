using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hosting;
using AkkaShopDemo.Actors.Checkout;
using Grpc.Core;
using Shop;
using static Shop.CheckoutService;

namespace AkkaShopDemo.Services 
{
    public class CheckoutService(IRequiredActor<CheckoutActor> checkoutActor) : CheckoutServiceBase
    {
        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            return await checkoutActor.ActorRef.Ask<PlaceOrderResponse>(request);
        }
    }
}
