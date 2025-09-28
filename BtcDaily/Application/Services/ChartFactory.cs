using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;

namespace BtcDaily
{
    public static class ChartFactory
    {
        public static Chart CreatePriceChart(string chartName, List<(System.DateTime Time, double Price)> prices)
        {
            var chart = new Chart();
            chart.Dock = DockStyle.Fill;

            // ChartArea
            var chartArea = new ChartArea { Name = chartName };
            chartArea.AxisX.LabelStyle.Format = "dd/MM\nHH:mm";
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY.Title = "Price ($)";
            chartArea.AxisY.LabelStyle.Format = "N2";
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            // Calculate Y-axis bounds
            double minPrice = prices.Min(p => p.Price);
            double maxPrice = prices.Max(p => p.Price);
            double buffer = (maxPrice - minPrice) * 0.002;
            chartArea.AxisY.Minimum = Math.Max(0, minPrice - buffer);
            chartArea.AxisY.Maximum = maxPrice + buffer;

            chart.ChartAreas.Add(chartArea);

            // Series
            var series = new Series("Price")
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                BorderWidth = 3,
                Color = prices.Last().Price < prices.First().Price
                        ? System.Drawing.Color.Red
                        : System.Drawing.Color.Green,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 5
            };

            foreach (var p in prices)
            {
                series.Points.AddXY(p.Time, p.Price);
            }

            chart.Series.Add(series);

            // Tooltip
           /* chart.GetToolTipText += (s, e) =>
            {
                if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
                {
                    var point = e.HitTestResult.Series.Points[e.HitTestResult.PointIndex];
                    DateTime time = DateTime.FromOADate(point.XValue);
                    double price = point.YValues[0];
                    e.Text = $"Date: {time:dd.MM HH:mm}\nPrice: ${price:N2}";
                    Debug.WriteLine($"Tooltip: {e.Text}");
                }
            };*/

            return chart;
        }
    }
}
