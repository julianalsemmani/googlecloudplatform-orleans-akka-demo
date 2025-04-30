using System;
using System.Security.Cryptography;
using System.Text;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Shop;

namespace AkkaShopDemo.Actors.Shipping
{
    public class ShippingActor : ReceiveActor
    {   
        private readonly Guid primaryKey = Guid.NewGuid();
        private readonly ILogger<ShippingActor> logger;
        public ShippingActor(ILogger<ShippingActor> logger)
        {
            this.logger = logger;

            Receive<GetQuoteRequest>(GetQuote);
            Receive<ShipOrderRequest>(ShipOrder);
        }

        public void GetQuote(GetQuoteRequest request)
        {
            logger.LogInformation("Getting quote for {Items}", request.Items);

            Sender.Tell(new GetQuoteResponse {
                CostUsd = new Money {
                    CurrencyCode = "USD",
                    Units = request.Items.Count * 2,
                    Nanos = 990000000
                }
            });
        }

        public void ShipOrder(ShipOrderRequest request)
        {
            logger.LogInformation("Shipping order requested with {Id}", primaryKey.ToString());
            logger.LogInformation("Shipping order for {Address}", request.Address);

            var address = request.Address.StreetAddress + request.Address.City + request.Address.State;
            var bytes = MD5.HashData(Encoding.UTF8.GetBytes(address));
            var trackingId = Convert.ToHexStringLower(bytes);

            Sender.Tell(new ShipOrderResponse {
                TrackingId = trackingId
            });
        }
    }
}
