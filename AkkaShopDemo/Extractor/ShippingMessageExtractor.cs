using Akka.Cluster.Sharding;
using Shop;
using System;

namespace AkkaShopDemo.Extractor
{
    public class ShippingMessageExtractor : IMessageExtractor
    {
        private const int NumShards = 10;

        public string EntityId(object message) => message switch
        {
            ShipOrderRequest m => m.Address?.StreetAddress ?? "unknown", // eller annen identifikator
            _ => "singleton"
        };

        public object EntityMessage(object message) => message;

        public string ShardId(object message)
        {
            var entityId = EntityId(message);
            return (Math.Abs(entityId.GetHashCode()) % NumShards).ToString();
        }

        public string ShardId(string entityId, object? messageHint = null)
        {
            return (Math.Abs(entityId.GetHashCode()) % NumShards).ToString();
        }
    }
}
