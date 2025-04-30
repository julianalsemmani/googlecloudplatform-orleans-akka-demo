using System;
using System.Threading.Tasks;
using Grpc.Core;
using Orleans;
using OrleansShopDemo.Grains.ProductCatalog.Interfaces;
using Shop;
using static Shop.ProductCatalogService;

namespace OrleansShopDemo.Services 
{
    public class ProductCatalogService(IClusterClient clusterClient) : ProductCatalogServiceBase
    {
        public override async Task<ListProductsResponse> ListProducts(Empty request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<IProductCatalogGrain>(Guid.NewGuid()).ListProducts();
        }

        public override async Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<IProductCatalogGrain>(Guid.NewGuid()).GetProduct(request);
        }

        public override async Task<SearchProductsResponse> SearchProducts(SearchProductsRequest request, ServerCallContext context)
        {
            return await clusterClient.GetGrain<IProductCatalogGrain>(Guid.NewGuid()).SearchProducts(request);
        }
    }
}
