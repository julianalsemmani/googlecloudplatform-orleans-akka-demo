using Akka.Cluster.Sharding;
using Shop;
using System;

namespace AkkaShopDemo.Extractor
{
    public class PaymentMessageExtractor : IMessageExtractor
    {
        private const int NumShards = 10;

        public string EntityId(object message) => message switch
        {
            ChargeRequest m => m.CreditCard?.CreditCardNumber ?? "unknown",
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
