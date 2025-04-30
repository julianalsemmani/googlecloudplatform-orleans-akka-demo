using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Shop;

namespace AkkaShopDemo.Actors.Cart
{
    public class CartActor : ReceiveActor
    {
        private readonly Guid primaryKey = Guid.NewGuid();
        private readonly Dictionary<string, Shop.Cart> carts = [];
        private readonly ILogger<CartActor> logger;
        public CartActor(ILogger<CartActor> logger)
        {
            this.logger = logger;

            Receive<AddItemRequest>(AddItem);
            Receive<EmptyCartRequest>(EmptyCart);
            Receive<GetCartRequest>(GetCart);
        }

        public void AddItem(AddItemRequest request)
        {
            logger.LogInformation("AddItem requested with {Id}", primaryKey.ToString());
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
        }

        public void EmptyCart(EmptyCartRequest request)
        {
            logger.LogInformation("EmptyCart requested with {Id}", primaryKey.ToString());
            logger.LogInformation("Emptying cart for {UserId}", request.UserId);

            if (carts.ContainsKey(request.UserId))
            {
                carts[request.UserId].Items.Clear();
            }
        }

        public void GetCart(GetCartRequest request)
        {
            logger.LogInformation("GetCart activated with {Id}", primaryKey.ToString());
            logger.LogInformation("Getting cart for {UserId}", request.UserId);

            Sender.Tell(carts.ContainsKey(request.UserId) ? carts[request.UserId] : new Shop.Cart());
        }
    }
}
