using Akka.Cluster.Sharding;
using Shop;
using System;

namespace AkkaShopDemo.Extractor
{
    public class CheckoutMessageExtractor : IMessageExtractor
    {
        private const int NumShards = 10;

        public string EntityId(object message)
        {
            return message switch
            {
                PlaceOrderRequest msg => msg.UserId,
                _ => null
            };
        }

        public object EntityMessage(object message) => message;

        public string ShardId(object message)
        {
            var entityId = EntityId(message);
            return entityId is null ? null : (Math.Abs(entityId.GetHashCode()) % NumShards).ToString();
        }

        public string ShardId(string entityId, object? messageHint = null)
        {
            return (Math.Abs(entityId.GetHashCode()) % NumShards).ToString();
        }
    }
}
