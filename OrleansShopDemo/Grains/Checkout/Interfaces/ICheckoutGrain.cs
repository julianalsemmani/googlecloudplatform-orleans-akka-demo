using System.Threading.Tasks;
using Orleans;
using Shop;

namespace OrleansShopDemo.Grains.Checkout.Interfaces
{
    [Alias("OrleansShopDemo.Grains.Checkout.Interfaces.ICheckoutGrain")]
    public interface ICheckoutGrain : IGrainWithGuidKey
    {
        [Alias("PlaceOrder")]
        public Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request);
    }
}
