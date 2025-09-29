using BtcDaily.Domain.Entities;
using BtcDaily.Domain.Interfaces;
using Newtonsoft.Json.Linq;
using System;


namespace BtcDaily.Infrastructure.Repositories
{
    public class CoinGeckoPriceFetcher : IPriceFetcher
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<List<PricePoint>> GetPricesForPeriodAsync(Currency currency, TimeRange range)
        {
            string daysParam = range switch
            {
                TimeRange.OneDay => "1",
                TimeRange.FiveDays => "5",
                TimeRange.OneMonth => "30",
                _ => "1"
            };

            string url = $"https://api.coingecko.com/api/v3/coins/{currency.Id}/market_chart?vs_currency=usd&days={daysParam}";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);

            var prices = data["prices"];
            var list = new List<PricePoint>();

            if (prices != null)
            {
                foreach (var item in prices)
                {
                    long timestamp = item[0].ToObject<long>();
                    double price = item[1].ToObject<double>();

                    DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;

                    list.Add(new PricePoint(time, price));
                }
            }
            return list;
        }
    }
}
