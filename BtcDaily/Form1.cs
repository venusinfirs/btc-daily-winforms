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
        private readonly Chart btcChart = new Chart();

        private const string ChartAreaName = "BTC prices for last 24 hours";
        private const double BufferPercentage = 0.002; 

        private readonly ToolTip customChartToolTip = new ToolTip(); 


        public Form1()
        {
            InitializeComponent();

            InitializeChartControl();

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
                PlottChart(sortedPrices);
            }
        }

        private void InitializeChartControl()
        {
            btcChart.Location = new System.Drawing.Point(10, 50);
            btcChart.Size = new System.Drawing.Size(800, 450);
            btcChart.Dock = DockStyle.Fill;

            this.Controls.Add(btcChart);

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

        private async void button1_Click(object sender, EventArgs e)
        {
            var btcPrices = await FetchAndPlotPricesAsync();
            var formattedPrices = btcPrices.Replace(",", ".");
            var sortedPrices = ParseAndSortPrices(formattedPrices);

            PlottChart(sortedPrices);
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

        private void PlottChart(List<(DateTime Time, double Price)> sortedPrices)
        {
            Chart chartControl = this.btcChart;

            chartControl.Series.Clear();
            chartControl.ChartAreas.Clear();

            ChartArea chartArea1 = new ChartArea();
            chartArea1.Name = ChartAreaName;

            chartArea1.AxisX.Interval = double.NaN; 
            chartArea1.AxisX.IntervalType = DateTimeIntervalType.Auto;

            chartArea1.AxisX.Interval = double.NaN; 
            chartArea1.AxisX.IntervalType = DateTimeIntervalType.Hours; 
            chartArea1.AxisX.LabelStyle.Format = "dd/MM\nHH:mm"; 
            chartArea1.AxisX.LabelStyle.Angle = 0;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            chartArea1.AxisY.Title = "Price ($)";
            chartArea1.AxisY.LabelStyle.Format = "N2";
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            var bounds = GetYAxisBounds(sortedPrices);

            chartArea1.AxisY.Minimum = bounds.min;
            chartArea1.AxisY.Maximum = bounds.max;

            chartControl.ChartAreas.Add(chartArea1);

            Series series1 = CreateChartSeries(sortedPrices);

            series1.ChartArea = ChartAreaName; 

            chartControl.Series.Add(series1);
        }

        private (double min, double max) GetYAxisBounds(List<(DateTime Time, double Price)> sortedPrices)
        {
            double minPrice = sortedPrices.Min(p => p.Price);
            double maxPrice = sortedPrices.Max(p => p.Price);

            double priceRange = maxPrice - minPrice;
            double buffer = priceRange * BufferPercentage;

            double axisMin = minPrice - buffer;
            double axisMax = maxPrice + buffer;

            if (axisMin < 0) axisMin = 0;
            return (axisMin, axisMax);
        }

        private Series CreateChartSeries(List<(DateTime Time, double Price)> sortedPrices)
        {
            Series series1 = new Series();
            series1.Name = "Price";
            series1.ChartType = SeriesChartType.Line;

            series1.XValueType = ChartValueType.DateTime; 

            series1.BorderWidth = 3;
           
            var isPriceInDecline = sortedPrices.Last().Price < sortedPrices.First().Price;

            series1.Color = isPriceInDecline ? System.Drawing.Color.Red : System.Drawing.Color.Green;

            series1.MarkerStyle = MarkerStyle.Circle; 
            series1.MarkerSize = 5;

            foreach (var (Time, Price) in sortedPrices)
            {
                series1.Points.AddXY(Time, Price);
            }

            return series1; 
        }
    }
}
