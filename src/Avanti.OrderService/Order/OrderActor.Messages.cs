namespace Avanti.OrderService.Order;

public partial class OrderActor
{
    public class GetOrderById
    {
        public int Id { get; set; }
    }

    public class InsertExternalOrder
    {
        public string ExternalId { get; set; } = "unknown";
        public string System { get; set; } = "unknown";
        public DateTimeOffset OrderDate { get; set; }
        public IEnumerable<OrderLine> Lines { get; set; } = Array.Empty<OrderLine>();

        public class OrderLine
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
        }
    }
}
