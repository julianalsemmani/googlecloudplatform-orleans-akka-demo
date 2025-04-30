using System.Threading.Tasks;
using Grpc.Core;
using Orleans;
using Shop;

namespace OrleansShopDemo.Grains.ProductCatalog.Interfaces
{
    [Alias("OrleansShopDemo.Grains.ProductCatalog.Interfaces.IProductCatalogGrain")]
    public interface IProductCatalogGrain : IGrainWithGuidKey
    {
        [Alias("ListProducts")]
        public Task<ListProductsResponse> ListProducts();

        [Alias("GetProduct")]
        public Task<Product> GetProduct(GetProductRequest request);

        [Alias("SearchProducts")]
        public Task<SearchProductsResponse> SearchProducts(SearchProductsRequest request);
    }
}
