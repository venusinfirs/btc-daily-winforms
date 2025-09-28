using BtcDaily.Domain.Entities;
using BtcDaily.Domain.Interfaces;

namespace BtcDaily.App.Services
{
    public class PriceService
    {
        private readonly IPriceFetcher _priceFetcher;

        public PriceService(IPriceFetcher priceFetcher)
        {
            _priceFetcher = priceFetcher;
        }

        public async Task<List<PricePoint>> GetPricesAsync(string currency, int days)
        {
            var prices = await _priceFetcher.GetPricesForPeriodAsync(currency, days);

            return prices.OrderBy(p => p.Time).ToList();
        }
    }
}
