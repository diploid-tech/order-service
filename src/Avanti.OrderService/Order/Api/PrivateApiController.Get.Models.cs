using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Avanti.OrderService.Order.Api
{
    public partial class PrivateApiController
    {
        public class GetOrderRequest
        {
            [Required]
            public int Id { get; set; }
        }

        public class GetOrderResponse
        {
            public int Id { get; set; }
            public DateTimeOffset OrderDate { get; set; }
            public IEnumerable<OrderLine> Lines { get; set; } = Array.Empty<OrderLine>();
            public IDictionary<string, string> ExternalIdentifiers { get; } = new Dictionary<string, string>();

            public class OrderLine
            {
                public int ProductId { get; set; }
                public int Amount { get; set; }
            }
        }
    }
}
