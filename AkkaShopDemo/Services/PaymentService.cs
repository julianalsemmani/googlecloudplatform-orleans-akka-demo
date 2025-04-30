using Akka.Actor;
using Akka.Hosting;
using AkkaShopDemo.Actors.Payment;
using Grpc.Core;
using Shop;
using System.Threading.Tasks;
using static Shop.PaymentService;

namespace AkkaShopDemo.Services
{
    public class PaymentService(IRequiredActor<PaymentActor> paymentActor) : PaymentServiceBase
    {
        public override async Task<ChargeResponse> Charge(ChargeRequest request, ServerCallContext context)
        {
            return await paymentActor.ActorRef.Ask<ChargeResponse>(request);
        }
    }
}
