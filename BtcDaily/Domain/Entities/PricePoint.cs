namespace BtcDaily.Domain.Entities
{
    public class PricePoint
    {
        public DateTime Time { get; }
        public double Price { get; }

        public PricePoint(DateTime time, double price)
        {
            Time = time;
            Price = price;
        }
    }
}
