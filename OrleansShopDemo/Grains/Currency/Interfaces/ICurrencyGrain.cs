using System.Threading.Tasks;
using Orleans;
using Shop;

namespace OrleansShopDemo.Grains.Currency.Interfaces
{
    [Alias("OrleansShopDemo.Grains.Currency.Interfaces.ICurrencyGrain")]
    public interface ICurrencyGrain : IGrainWithGuidKey
    {
        [Alias("GetSupportedCurrencies")]
        public Task<GetSupportedCurrenciesResponse> GetSupportedCurrencies();
        
        [Alias("Convert")]
        public Task<Money> Convert(CurrencyConversionRequest request);
    }
}
