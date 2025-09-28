using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using BtcDaily.Domain.Entities;

namespace BtcDaily.App.Services
{
    public static class ChartFactory
    {
        private const double MaxBufferPercentage = 0.1;
        private const double MinBufferPercentage = 0.05;
        public static Chart CreatePriceChart(string chartName, List<PricePoint> prices)
        {
            var chart = new Chart();
            chart.Dock = DockStyle.Fill;

            var chartArea = new ChartArea { Name = chartName };
            chartArea.AxisX.LabelStyle.Format = "dd/MM\nHH:mm";
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY.Title = "Price ($)";
            chartArea.AxisY.LabelStyle.Format = "N2";
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            double minPrice = prices.Min(p => p.Price);
            double maxPrice = prices.Max(p => p.Price);
            double maxBuffer = (maxPrice - minPrice) * MaxBufferPercentage;
            double minBuffer = (maxPrice - minPrice) * MinBufferPercentage;
            chartArea.AxisY.Minimum = Math.Max(0, minPrice - minBuffer);
            chartArea.AxisY.Maximum = maxPrice + maxBuffer;

            chart.ChartAreas.Add(chartArea);

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
            return chart;
        }
    }
}
