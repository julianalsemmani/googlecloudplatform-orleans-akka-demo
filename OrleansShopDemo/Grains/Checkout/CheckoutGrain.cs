using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansShopDemo.Grains.Cart.Constants;
using OrleansShopDemo.Grains.Cart.Interfaces;
using OrleansShopDemo.Grains.Checkout.Interfaces;
using OrleansShopDemo.Grains.Currency;
using OrleansShopDemo.Grains.Currency.Interfaces;
using OrleansShopDemo.Grains.Payment.Interfaces;
using OrleansShopDemo.Grains.ProductCatalog.Interfaces;
using OrleansShopDemo.Grains.Shipping.Interfaces;
using OrleansShopDemo.Utils;
using Shop;

namespace OrleansShopDemo.Grains.Checkout
{
    public class CheckoutGrain : Grain, ICheckoutGrain
    {
        private readonly IClusterClient clusterClient;
        private readonly ILogger<CheckoutGrain> logger;
        public CheckoutGrain(IClusterClient clusterClient, ILogger<CheckoutGrain> logger)
        {
            this.clusterClient = clusterClient;
            this.logger = logger;
        }

        public async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request)
        {  
            logger.LogInformation("PlaceOrder requested with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Placing order for {UserId}", request.UserId);

            PlaceOrderResponse orderResponse = new() { Order = new OrderResult() { OrderId = Guid.NewGuid().ToString(), ShippingAddress = request.Address } };

            Shop.Cart cart = await GetCart(request.UserId);

            Money shippingCost = await GetShippingQuote(cart, request.Address);
            orderResponse.Order.ShippingCost = await ConvertCurrency(shippingCost, request.UserCurrency);

            OrderItem[] orderItems = await Task.WhenAll(cart.Items.Select(c => CreateOrderItem(c, request.UserCurrency)));
            orderResponse.Order.Items.AddRange(orderItems);

            await clusterClient.GetGrain<IPaymentGrain>(Guid.NewGuid()).Charge(new ChargeRequest { CreditCard = request.CreditCard, Amount = new Money { CurrencyCode = request.UserCurrency, Units = orderResponse.Order.Items.Sum(o => o.Cost.Units), Nanos = 0 } });

            string shippingTrackingId = await OrderShipping(cart, request.Address);
            orderResponse.Order.ShippingTrackingId = shippingTrackingId;

            return orderResponse;
        }

        private async Task<Shop.Cart> GetCart(string userId)
        {
            logger.LogInformation("GetCart activated with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Getting cart for {UserId}", userId);

            ICartGrain cartGrain = clusterClient.GetGrain<ICartGrain>(CartGrainId.Id);
            return await cartGrain.GetCart(new GetCartRequest { UserId = userId });
        }

        private async Task<Money> GetShippingQuote(Shop.Cart cart, Address address)
        {
            logger.LogInformation("GetShippingQuote activated with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Getting shipping quote for {Items}", cart.Items);

            IShippingGrain shippingGrain = clusterClient.GetGrain<IShippingGrain>(Guid.NewGuid());

            GetQuoteRequest quoteRequest = new() { Address = address };
            quoteRequest.Items.AddRange(cart.Items);

            GetQuoteResponse quoteResponse = await shippingGrain.GetQuote(quoteRequest);
            return quoteResponse.CostUsd;
        }

        private async Task<Product> GetProduct(string productId)
        {
            logger.LogInformation("GetProduct activated with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Getting product for {ProductId}", productId);

            IProductCatalogGrain productCatalogGrain = clusterClient.GetGrain<IProductCatalogGrain>(Guid.NewGuid());
            return await productCatalogGrain.GetProduct(new GetProductRequest { Id = productId });
        }

        private async Task<Money> ConvertCurrency(Money money, string toCurrencyCode)
        {
            logger.LogInformation("ConvertCurrency activated with {Id}", this.GetPrimaryKey().ToString());
            logger.LogInformation("Converting {From} to {To}", money, toCurrencyCode);

            ICurrencyGrain currencyGrain = clusterClient.GetGrain<ICurrencyGrain>(Guid.NewGuid());
            return await currencyGrain.Convert(new CurrencyConversionRequest() { From = money, ToCode = toCurrencyCode });
        }

        private async Task<string> OrderShipping(Shop.Cart cart, Address address)
        {
            IShippingGrain shippingGrain = clusterClient.GetGrain<IShippingGrain>(Guid.NewGuid());

            ShipOrderRequest orderRequest = new () { Address = address };
            orderRequest.Items.AddRange(cart.Items);

            ShipOrderResponse orderResponse = await shippingGrain.ShipOrder(orderRequest);
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
