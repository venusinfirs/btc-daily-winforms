namespace BtcDaily.Domain.Entities
{
    public class PricePoint
    {
        public DateTime Time { get; }
        public decimal Price { get; }

        public PricePoint(DateTime time, decimal price)
        {
            Time = time;
            Price = price;
        }
    }
}
