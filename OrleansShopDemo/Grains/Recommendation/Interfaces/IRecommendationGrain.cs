using System.Threading.Tasks;
using Orleans;
using Shop;

namespace OrleansShopDemo.Grains.Recommendation.Interfaces
{
    [Alias("OrleansShopDemo.Grains.Recommendation.Interfaces.IRecommendationGrain")]
    public interface IRecommendationGrain : IGrainWithGuidKey
    {
        [Alias("ListRecommendations")]
        public Task<ListRecommendationsResponse> ListRecommendations(ListRecommendationsRequest request);
    }
}
