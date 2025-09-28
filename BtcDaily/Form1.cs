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
    }
}
