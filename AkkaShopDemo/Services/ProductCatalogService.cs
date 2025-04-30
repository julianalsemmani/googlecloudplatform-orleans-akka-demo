using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hosting;
using AkkaShopDemo.Actors.ProductCatalog;
using Grpc.Core;
using Shop;
using static Shop.ProductCatalogService;

namespace AkkaShopDemo.Services 
{
    public class ProductCatalogService(IRequiredActor<ProductCatalogActor> productCatalogActor) : ProductCatalogServiceBase
    {
        public override async Task<ListProductsResponse> ListProducts(Empty request, ServerCallContext context)
        {
            return await productCatalogActor.ActorRef.Ask<ListProductsResponse>(new ListProductsRequest());
        }

        public override async Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            return await productCatalogActor.ActorRef.Ask<Product>(request);
        }

        public override async Task<SearchProductsResponse> SearchProducts(SearchProductsRequest request, ServerCallContext context)
        {
            return await productCatalogActor.ActorRef.Ask<SearchProductsResponse>(request);
        }
    }
}
