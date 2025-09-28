using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;

namespace BtcDaily
{
    public partial class Form1 : Form
    {

        private string _allPrices;
        private readonly CryptoPriceFetcher priceFetcher = new CryptoPriceFetcher();
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var btcPrices = await FetchAndPlotPricesAsync();
            var formattedPrices = btcPrices.Replace(",", ".");
            ParseAndSortPrices(formattedPrices);
           // CreateNewLineChart();
        }

        public List<(DateTime Time, double Price)> ParseAndSortPrices(string rawData)
        {
            var sortedPrices = new List<(DateTime Time, double Price)>();

            string[] lines = rawData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                try
                {
                    string[] parts = line.Split(new string[] { " - $" }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2)
                    {
                        string dateTimeString = parts[0].Trim();
                        string priceString = parts[1].Trim();

                        DateTime timePoint = DateTime.ParseExact(dateTimeString,
                                                      "dd.MM HH:mm", // <-- Use dot separator
                                                      CultureInfo.InvariantCulture);

                        double priceValue = double.Parse(priceString, CultureInfo.InvariantCulture);

                        sortedPrices.Add((Time: timePoint, Price: priceValue));
                    }
                }
                catch (FormatException ex)
                {
                    Debug.WriteLine($"Error parsing line: {line}. Error: {ex.Message}");
                }
            }

            var orderedList = sortedPrices.OrderBy(item => item.Time).ToList();

            foreach (var item in orderedList)
            {
                Debug.WriteLine($"Sorted prices: {item.Time:HH:mm} - ${item.Price:F2}");
            }

            return orderedList;
        }

        private async Task<string> FetchAndPlotPricesAsync()
        {
            string prices = string.Empty; 

            try
            {
                
                prices = await priceFetcher.GetBtcPricesFor1DayAsync();

                label1.Text = prices;

                //Debug.WriteLine(prices); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching BTC prices: {ex.Message}");
                Debug.WriteLine($"Error: {ex.Message}");

                prices = "Failed to fetch data.";
            }
            return prices;
        }
    }
}
