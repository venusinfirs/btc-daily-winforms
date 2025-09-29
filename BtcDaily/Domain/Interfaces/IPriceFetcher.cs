using BtcDaily.Domain.Entities;

namespace BtcDaily.Domain.Interfaces
{
    public interface IPriceFetcher
    {
        Task<List<PricePoint>> GetPricesForPeriodAsync(Currency currency, TimeRange range);
    }
}
