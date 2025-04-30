using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hosting;
using AkkaShopDemo.Actors.ProductCatalog;
using Microsoft.Extensions.Logging;
using Shop;

namespace AkkaShopDemo.Actors.Recommendation
{
    public class RecommendationActor : ReceiveActor
    {
        private readonly Guid primaryKey = Guid.NewGuid();
        private static readonly int maxRecommendations = 5;
        private readonly ILogger<RecommendationActor> logger;
        private readonly IActorRef productCatalogActorRef;

        public RecommendationActor(ILogger<RecommendationActor> logger, IRequiredActor<ProductCatalogActor> productCatalogActor)
        {
            this.logger = logger;
            this.productCatalogActorRef = productCatalogActor.ActorRef;

            ReceiveAsync<ListRecommendationsRequest>(ListRecommendations);
        }

        public async Task ListRecommendations(ListRecommendationsRequest request)
        {
            logger.LogInformation("RecommendationActor activated with {id}", primaryKey.ToString());
            logger.LogInformation("Listing recommendations for {ProductIds}", request.ProductIds);
            
            var allProducts = await productCatalogActorRef.Ask<ListProductsResponse>(new ListProductsRequest());
            var recommendedProductIds = allProducts.Products.Where(p => !request.ProductIds.Contains(p.Id)).Select(p => p.Id).ToList();

            if (recommendedProductIds.Count > maxRecommendations)
            {
                recommendedProductIds.RemoveRange(0, recommendedProductIds.Count - maxRecommendations);
            }

            var response = new ListRecommendationsResponse();
            response.ProductIds.AddRange(recommendedProductIds);

            Sender.Tell(response);
        }
    }
}
