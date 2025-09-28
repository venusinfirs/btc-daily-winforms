using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Policy;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace BtcDaily
{
    public partial class Form1 : Form
    {
        private readonly CryptoPriceFetcher priceFetcher = new CryptoPriceFetcher();
        private Chart? btcChart;

        private const string ChartAreaName = "BTC prices for last 24 hours";
        private const double BufferPercentage = 0.002; 

        private readonly ToolTip customChartToolTip = new ToolTip(); 


        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            var btcPrices = await FetchAndPlotPricesAsync();

            if (btcPrices == "Failed to fetch data.")
            {
                return;
            }

            var formattedPrices = btcPrices.Replace(",", ".");
            var sortedPrices = ParseAndSortPrices(formattedPrices);

            if (sortedPrices.Count > 0)
            {
                CreateAndDisplayChart(sortedPrices);
            }
        }

        private void CreateAndDisplayChart(List<(DateTime Time, double Price)> sortedPrices)
        {
            if (btcChart != null)
            {
                this.Controls.Remove(btcChart);
                btcChart.Dispose();
            }

            btcChart = ChartFactory.CreatePriceChart("BTC prices for last 24 hours", sortedPrices);

            this.Controls.Add(btcChart);
            btcChart.BringToFront();
            CreateTooltip();
        }

        private void CreateTooltip() 
        {
            if (btcChart == null) return;

            btcChart.GetToolTipText += (s, e) =>
            {
                if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
                {
                    Series series = e.HitTestResult.Series;

                    if (series != null)
                    {

                        if (e.HitTestResult.PointIndex >= 0 && e.HitTestResult.PointIndex < series.Points.Count)
                        {
                            DataPoint point = series.Points[e.HitTestResult.PointIndex];

                            DateTime time = DateTime.FromOADate(point.XValue);
                            double price = point.YValues[0];

                            e.Text = $"Date: {time.ToString("dd.MM HH:mm", CultureInfo.InvariantCulture)}\nPrice: ${price.ToString("N2", CultureInfo.InvariantCulture)}";

                            Debug.WriteLine($"Tooltip Text Generated: {e.Text}");
                            toolTip1.BackColor = System.Drawing.Color.LightYellow;
                            toolTip1.Show(e.Text, btcChart, e.X, e.Y - 15);
                        }
                        else
                        {
                            e.Text = string.Empty;
                        }
                    }
                    else
                    {
                        e.Text = string.Empty;
                    }
                }
                else
                {
                    e.Text = string.Empty;
                }
            };
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
                                                      "dd.MM HH:mm", 
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

            return orderedList;
        }

        private async Task<string> FetchAndPlotPricesAsync()
        {
            string prices = string.Empty;

            try
            {

                prices = await priceFetcher.GetBtcPricesFor1DayAsync();

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
