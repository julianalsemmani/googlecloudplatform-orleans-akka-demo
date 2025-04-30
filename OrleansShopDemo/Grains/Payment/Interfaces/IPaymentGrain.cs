using Orleans;
using Shop;
using System.Threading.Tasks;

namespace OrleansShopDemo.Grains.Payment.Interfaces
{
    public interface IPaymentGrain : IGrainWithGuidKey
    {
        [Alias("Charge")]
        public Task<ChargeResponse> Charge(ChargeRequest request);
    }
}
