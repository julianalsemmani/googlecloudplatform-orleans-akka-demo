using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AkkaShopDemo.Utils;
using Shop;
using Akka.Hosting;
using AkkaShopDemo.Actors.Cart;
using Akka.Actor;
using AkkaShopDemo.Actors.Payment;
using AkkaShopDemo.Actors.Shipping;
using AkkaShopDemo.Actors.ProductCatalog;
using AkkaShopDemo.Actors.Currency;

namespace AkkaShopDemo.Actors.Checkout
{
    public class CheckoutActor : ReceiveActor
    {
        private readonly Guid primaryKey = Guid.NewGuid();
        private readonly IActorRef cartActorRef;
        private readonly IActorRef paymentActorRef;
        private readonly IActorRef shippingActorRef;
        private readonly IActorRef productCatalogActorRef;
        private readonly IActorRef currencyActorRef;
        private readonly ILogger<CheckoutActor> logger;
        public CheckoutActor(IRequiredActor<CartActor> cartActor, IRequiredActor<PaymentActor> paymentActor, IRequiredActor<ShippingActor> shippingActor, IRequiredActor<ProductCatalogActor> productCatalogActor, IRequiredActor<CurrencyActor> currencyActor, ILogger<CheckoutActor> logger)
        {
            this.cartActorRef = cartActor.ActorRef;
            this.paymentActorRef = paymentActor.ActorRef;
            this.shippingActorRef = shippingActor.ActorRef;
            this.productCatalogActorRef = productCatalogActor.ActorRef;
            this.currencyActorRef = currencyActor.ActorRef;
            this.logger = logger;

            ReceiveAsync<PlaceOrderRequest>(PlaceOrder);
        }

        public async Task PlaceOrder(PlaceOrderRequest request)
        {  
            logger.LogInformation("PlaceOrder requested with {Id}", primaryKey.ToString());
            logger.LogInformation("Placing order for {UserId}", request.UserId);

            PlaceOrderResponse orderResponse = new() { Order = new OrderResult() { OrderId = Guid.NewGuid().ToString(), ShippingAddress = request.Address } };

            Shop.Cart cart = await GetCart(request.UserId);

            Money shippingCost = await GetShippingQuote(cart, request.Address);
            orderResponse.Order.ShippingCost = await ConvertCurrency(shippingCost, request.UserCurrency);

            OrderItem[] orderItems = await Task.WhenAll(cart.Items.Select(c => CreateOrderItem(c, request.UserCurrency)));
            orderResponse.Order.Items.AddRange(orderItems);

            await paymentActorRef.Ask(new ChargeRequest { CreditCard = request.CreditCard, Amount = new Money { CurrencyCode = request.UserCurrency, Units = orderResponse.Order.Items.Sum(o => o.Cost.Units), Nanos = 0 } });

            string shippingTrackingId = await OrderShipping(cart, request.Address);
            orderResponse.Order.ShippingTrackingId = shippingTrackingId;

            Sender.Tell(orderResponse);
        }

        private async Task<Shop.Cart> GetCart(string userId)
        {
            logger.LogInformation("GetCart activated with {Id}", primaryKey.ToString());
            logger.LogInformation("Getting cart for {UserId}", userId);

            return await cartActorRef.Ask<Shop.Cart>(new GetCartRequest { UserId = userId });
        }

        private async Task<Money> GetShippingQuote(Shop.Cart cart, Shop.Address address)
        {
            logger.LogInformation("GetShippingQuote activated with {Id}", primaryKey.ToString());
            logger.LogInformation("Getting shipping quote for {Items}", cart.Items);

            GetQuoteRequest quoteRequest = new() { Address = address };
            quoteRequest.Items.AddRange(cart.Items);

            GetQuoteResponse quoteResponse = await shippingActorRef.Ask<GetQuoteResponse>(quoteRequest);
            return quoteResponse.CostUsd;
        }

        private async Task<Product> GetProduct(string productId)
        {
            logger.LogInformation("GetProduct activated with {Id}", primaryKey.ToString());
            logger.LogInformation("Getting product for {ProductId}", productId);

            return await productCatalogActorRef.Ask<Product>(new GetProductRequest { Id = productId });
        }

        private async Task<Money> ConvertCurrency(Money money, string toCurrencyCode)
        {
            logger.LogInformation("ConvertCurrency activated with {Id}", primaryKey.ToString());
            logger.LogInformation("Converting {From} to {To}", money, toCurrencyCode);

            return await currencyActorRef.Ask<Money>(new CurrencyConversionRequest() { From = money, ToCode = toCurrencyCode });
        }

        private async Task<string> OrderShipping(Shop.Cart cart, Shop.Address address)
        {
            ShipOrderRequest orderRequest = new () { Address = address };
            orderRequest.Items.AddRange(cart.Items);

            ShipOrderResponse orderResponse = await shippingActorRef.Ask<ShipOrderResponse>(orderRequest);
            return orderResponse.TrackingId;
        }

        private async Task<OrderItem> CreateOrderItem(CartItem cartItem, string currencyCode)
        {
            Product product = await GetProduct(cartItem.ProductId);
            Money cost = CalculateCost(product, cartItem.Quantity);

            return new OrderItem { 
                Item = cartItem,
                Cost = await ConvertCurrency(cost, currencyCode)
            };
        }

        private static Money CalculateCost(Product product, int quantity)
        {
            decimal price = MoneyUtil.CombinePrice(product.PriceUsd.Units, product.PriceUsd.Nanos);
            price *= quantity;

            Tuple<long, int> cost = MoneyUtil.SplitPrice(price);

            return new Money {
                CurrencyCode = product.PriceUsd.CurrencyCode,
                Units = cost.Item1,
                Nanos = cost.Item2
            };
        }
    }
}
