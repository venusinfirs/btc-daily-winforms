using BtcDaily.App.Services;
using BtcDaily.Domain.Entities;
using BtcDaily.Infrastructure.Repositories;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;



namespace BtcDaily
{
    public partial class Form1 : Form
    {
        private readonly CryptoPriceFetcher priceFetcher = new CryptoPriceFetcher();
        private readonly PriceService _priceService;

        private Chart? btcChart;

        private const string ChartAreaName = "BTC prices for last 24 hours";
        private const double BufferPercentage = 0.002;
        private ProgressBar progressBar = new ProgressBar();

  

        public Form1()
        {
            InitializeComponent();

           
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Dock = DockStyle.Bottom;
            this.Controls.Add(progressBar);

            var fetcher = new CoinGeckoPriceFetcher();
            _priceService = new PriceService(fetcher);

            this.Load += Form1_Load;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            progressBar.Visible = true;
           
            var sortedPrices = await _priceService.GetPricesAsync("bitcoin", 1);
            progressBar.Visible = false;

            if (sortedPrices.Count > 0)
            {
                CreateAndDisplayChart(sortedPrices);
            }
        }

        private void CreateAndDisplayChart(List<PricePoint> sortedPrices)
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
                // Only show tooltip for data points
                if (e.HitTestResult.ChartElementType != ChartElementType.DataPoint)
                {
                    e.Text = string.Empty;
                    return;
                }

                var series = e.HitTestResult.Series;
                if (series == null)
                {
                    e.Text = string.Empty;
                    return;
                }

                int index = e.HitTestResult.PointIndex;
                if (index < 0 || index >= series.Points.Count)
                {
                    e.Text = string.Empty;
                    return;
                }

                var point = series.Points[index];
                DateTime time = DateTime.FromOADate(point.XValue);
                double price = point.YValues[0];

                string tooltipText = $"Date: {time:dd.MM HH:mm}\nPrice: ${price:N2}";

                e.Text = tooltipText;
                toolTip1.BackColor = System.Drawing.Color.LightYellow;
                toolTip1.Show(tooltipText, btcChart, e.X, e.Y - 15);
            };
        }
    }
}
