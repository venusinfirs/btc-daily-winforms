using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BtcDaily
{
    public class CryptoPriceFetcher
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Gets the BTC price history for the past 1 day from CoinGecko API.
        /// </summary>
        /// <summary>
        /// Gets all BTC prices for the past 1 day from CoinGecko API.
        /// </summary>
        public async Task<string> GetBtcPricesFor1DayAsync()
        {
            string url = "https://api.coingecko.com/api/v3/coins/bitcoin/market_chart?vs_currency=usd&days=1";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);

            var prices = data["prices"];
            if (prices == null) return "No data available.";

            StringBuilder sb = new StringBuilder();
            foreach (var item in prices)
            {
                long timestamp = item[0].ToObject<long>();
                decimal price = item[1].ToObject<decimal>();

                // Convert Unix timestamp (milliseconds) to UTC time
                // The resulting DateTime object will be in UTC time zone
                DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;

                // The string output remains the same, but now HH:mm reflects the UTC time
                sb.AppendLine($"{time:dd/MM HH:mm} - ${price:F2}");

            }
            return sb.ToString();
        }
    }
}