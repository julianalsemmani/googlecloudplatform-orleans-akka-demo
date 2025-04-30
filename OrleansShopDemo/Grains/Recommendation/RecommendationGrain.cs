using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansShopDemo.Grains.ProductCatalog.Interfaces;
using OrleansShopDemo.Grains.Recommendation.Interfaces;
using Shop;

namespace OrleansShopDemo.Grains.Recommendation
{
    public class RecommendationGrain : Grain, IRecommendationGrain
    {
        private static readonly int maxRecommendations = 5;
        private readonly ILogger<RecommendationGrain> logger;
        private readonly IClusterClient clusterClient;

        public RecommendationGrain(ILogger<RecommendationGrain> logger, IClusterClient clusterClient)
        {
            this.logger = logger;
            this.clusterClient = clusterClient;
        }

        public async Task<ListRecommendationsResponse> ListRecommendations(ListRecommendationsRequest request)
        {
            logger.LogInformation("RecommendationGrain activated with {id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Listing recommendations for {ProductIds}", request.ProductIds);
            
            var allProducts = await clusterClient.GetGrain<IProductCatalogGrain>(new Guid()).ListProducts();
            var recommendedProductIds = allProducts.Products.Where(p => !request.ProductIds.Contains(p.Id)).Select(p => p.Id).ToList();

            if (recommendedProductIds.Count > maxRecommendations)
            {
                recommendedProductIds.RemoveRange(0, recommendedProductIds.Count - maxRecommendations);
            }

            var response = new ListRecommendationsResponse();
            response.ProductIds.AddRange(recommendedProductIds);

            return response;
        }
    }
}
