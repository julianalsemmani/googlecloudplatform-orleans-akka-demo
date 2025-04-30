using System;
using System.Threading.Tasks;
using Grpc.Core;
using Orleans;
using OrleansShopDemo.Grains.Recommendation.Interfaces;
using Shop;
using static Shop.RecommendationService;

namespace OrleansShopDemo.Services 
{
    public class RecommendationService(IClusterClient clusterClient) : RecommendationServiceBase
    {
        public override async Task<ListRecommendationsResponse> ListRecommendations(ListRecommendationsRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<IRecommendationGrain>(Guid.NewGuid()).ListRecommendations(request);
        }
    }
}
