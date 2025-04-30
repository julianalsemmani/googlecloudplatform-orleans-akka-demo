using System.Threading.Tasks;
using Grpc.Core;
using Orleans;
using Shop;
namespace OrleansShopDemo.Grains.Cart.Interfaces
{
    [Alias("OrleansShopDemo.Grains.Cart.Interfaces.ICartGrain")]
    public interface ICartGrain : IGrainWithGuidKey
    {
        [Alias("AddItem")]
        public Task AddItem(AddItemRequest request);

        [Alias("GetCart")]
        public Task<Shop.Cart> GetCart(GetCartRequest request);

        [Alias("EmptyCart")]
        public Task EmptyCart(EmptyCartRequest request);
    }
}
