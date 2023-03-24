using Avanti.OrderService.Order.Documents;

namespace Avanti.OrderService.Order;

public partial class OrderActor
{
    public interface IResponse { }

    public class OrderFound : IResponse
    {
        public int Id { get; set; }
        public OrderDocument Document { get; set; } = new OrderDocument();
    }

    public class OrderNotFound : IResponse { }
    public class OrderRetrievalFailed : IResponse { }

    public class OrderInserted : IResponse
    {
        public int Id { get; set; }
    }

    public class OrderAlreadyExists : IResponse { }
    public class OrderFailedToStore : IResponse { }
}
