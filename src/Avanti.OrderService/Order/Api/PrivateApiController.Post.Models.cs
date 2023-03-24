using System.ComponentModel.DataAnnotations;
using Avanti.Core.Microservice.Web;

namespace Avanti.OrderService.Order.Api;

public partial class PrivateApiController
{
    public class PostOrderRequest
    {
        [Required]
        public string ExternalId { get; set; } = "unknown";

        [Required]
        public string System { get; set; } = "unknown";

        [Required]
        public DateTimeOffset OrderDate { get; set; }

        [Required]
        [MustHaveElements]
        public IEnumerable<OrderLine> Lines { get; set; } = Array.Empty<OrderLine>();

        public class OrderLine
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
        }
    }

    public class PostOrderResponse
    {
        public int Id { get; set; }
    }
}
