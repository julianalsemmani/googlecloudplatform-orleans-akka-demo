using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hosting;
using AkkaShopDemo.Actors.Recommendation;
using Grpc.Core;
using Shop;
using static Shop.RecommendationService;

namespace AkkaShopDemo.Services 
{
    public class RecommendationService(IRequiredActor<RecommendationActor> recommendationActor) : RecommendationServiceBase
    {
        public override async Task<ListRecommendationsResponse> ListRecommendations(ListRecommendationsRequest request, ServerCallContext context)
        {
            return await recommendationActor.ActorRef.Ask<ListRecommendationsResponse>(request);
        }
    }
}
