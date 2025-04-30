using Microsoft.Extensions.Logging;
using Orleans;
using OrleansShopDemo.Grains.Cart.Interfaces;
using Shop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrleansShopDemo.Grains.Cart
{
    public class CartGrain : Grain, ICartGrain
    {
        private readonly Dictionary<string, Shop.Cart> carts = [];
        private readonly ILogger<CartGrain> logger;
        public CartGrain(ILogger<CartGrain> logger)
        {
            this.logger = logger;
        }

        public Task AddItem(AddItemRequest request)
        {
            logger.LogInformation("AddItem requested with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Adding item to cart for {UserId}", request.UserId);

            if (!carts.ContainsKey(request.UserId))
            {
                carts[request.UserId] = new Shop.Cart();
            }

            var existingItem = carts[request.UserId].Items.FirstOrDefault(i => i.ProductId == request.Item.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Item.Quantity;
            }
            else
            {
                carts[request.UserId].Items.Add(request.Item);
            }

            return Task.CompletedTask;
        }

        public Task EmptyCart(EmptyCartRequest request)
        {
            logger.LogInformation("EmptyCart requested with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Emptying cart for {UserId}", request.UserId);

            if (carts.ContainsKey(request.UserId))
            {
                carts[request.UserId].Items.Clear();
            }

            return Task.CompletedTask;
        }

        public Task<Shop.Cart> GetCart(GetCartRequest request)
        {
            logger.LogInformation("GetCart activated with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Getting cart for {UserId}", request.UserId);

            return Task.FromResult(carts.ContainsKey(request.UserId) ? carts[request.UserId] : new Shop.Cart());
        }
    }
}
