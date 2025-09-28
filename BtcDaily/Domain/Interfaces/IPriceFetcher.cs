using BtcDaily.Domain.Entities;

namespace BtcDaily.Domain.Interfaces
{
    public interface IPriceFetcher
    {
        Task<List<PricePoint>> GetPricesForPeriodAsync(string currency, int days);
    }
}
