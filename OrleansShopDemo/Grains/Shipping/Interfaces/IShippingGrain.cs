using System.Threading.Tasks;
using Orleans;
using Shop;

namespace OrleansShopDemo.Grains.Shipping.Interfaces
{
    [Alias("OrleansShopDemo.Grains.Shipping.Interfaces.IShippingGrain")]
    public interface IShippingGrain : IGrainWithGuidKey
    {
        [Alias("GetQuote")]
        public Task<GetQuoteResponse> GetQuote(GetQuoteRequest request);

        [Alias("ShipOrder")]
        public Task<ShipOrderResponse> ShipOrder(ShipOrderRequest request);
    }
}
