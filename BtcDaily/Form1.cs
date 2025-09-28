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
        private const double BufferPercentage = 0.002; // 0.2% buffer for Y-axis scaling

        private readonly ToolTip customChartToolTip = new ToolTip(); // <-- New custom ToolTip


        public Form1()
        {
            InitializeComponent();

            InitializeChartControl();
        }

        private void InitializeChartControl()
        {
            // Configure the size and docking of the chart control
            btcChart.Location = new System.Drawing.Point(10, 50);
            btcChart.Size = new System.Drawing.Size(800, 450);
            btcChart.Dock = DockStyle.Fill;

            // Add the control to the Form's collection
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
                            toolTip1.Show(e.Text, btcChart, e.X, e.Y - 15); // Offset Y to avoid cursor overlap
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

            /*foreach (var item in sortedPrices)
            {
                Debug.WriteLine($"Final sorted prices: {item.Time} - ${item.Price:F2}");
            }*/
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

            /* foreach (var item in orderedList)
             {
                 Debug.WriteLine($"Sorted prices: {item.Time:HH:mm} - ${item.Price:F2}");
             }*/

            return orderedList;
        }

        private async Task<string> FetchAndPlotPricesAsync()
        {
            string prices = string.Empty;

            try
            {

                prices = await priceFetcher.GetBtcPricesFor1DayAsync();

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

        // In Form1.cs

        private void PlottChart(List<(DateTime Time, double Price)> sortedPrices)
        {
            // Reference the instantiated chart control
            Chart chartControl = this.btcChart;

            // 1. Clear previous elements (crucial for re-plotting)
            chartControl.Series.Clear();
            chartControl.ChartAreas.Clear();


            // 2. Create and configure ChartArea
            ChartArea chartArea1 = new ChartArea();
            chartArea1.Name = ChartAreaName;

            // Set Interval to Auto(or one day) and Type to Auto
            // This often allows the chart to choose the best interval (e.g., every 4 hours)
            // while ensuring the labels contain the date.
            chartArea1.AxisX.Interval = double.NaN; // Reset to Auto
            chartArea1.AxisX.IntervalType = DateTimeIntervalType.Auto;

            // X-axis settings (time)
            chartArea1.AxisX.Interval = double.NaN; // Let chart auto-select interval
            chartArea1.AxisX.IntervalType = DateTimeIntervalType.Hours; // ✅ FIX 1
            chartArea1.AxisX.LabelStyle.Format = "dd/MM\nHH:mm"; // ✅ FIX 2 (date+time)
            chartArea1.AxisX.LabelStyle.Angle = 0;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            // Set Y-Axis properties for Price data
            chartArea1.AxisY.Title = "Price ($)";
            chartArea1.AxisY.LabelStyle.Format = "N2";
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            var bounds = GetYAxisBounds(sortedPrices);

            chartArea1.AxisY.Minimum = bounds.min;
            chartArea1.AxisY.Maximum = bounds.max;

            // Add the ChartArea to the Chart Control
            chartControl.ChartAreas.Add(chartArea1);

            // 3. Create the Series and add data
            Series series1 = CreateChartSeries(sortedPrices);

            // 4. Assign the Series to the ChartArea
            series1.ChartArea = ChartAreaName; // Uses the name defined above

            // 5. Add the Series to the Chart Control
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

            series1.XValueType = ChartValueType.DateTime; // Tells the chart the X-data is time, not generic numeric

            series1.BorderWidth = 3;
            series1.Color = System.Drawing.Color.Blue;
            series1.MarkerStyle = MarkerStyle.Circle; // Adds a small circle marker to each data point
            series1.MarkerSize = 5;

            //  series1.ToolTip = "Date: #VALX{dd/MM HH:mm}\nPrice: $#VALY{F2}";


            foreach (var (Time, Price) in sortedPrices)
            {
                series1.Points.AddXY(Time, Price);
            }

            return series1; // Adds the configured series to the chart
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }
    }
}
