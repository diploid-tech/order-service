using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Avanti.OrderService.Order.Documents
{
    public class OrderDocument
    {
        public DateTimeOffset OrderDate { get; set; }
        public IEnumerable<OrderLine> Lines { get; set; } = Array.Empty<OrderLine>();
        public IImmutableDictionary<string, string>? ExternalIdentifiers { get; set; }

        public class OrderLine
        {
            public int ProductId { get; set; }
            public int Amount { get; set; }
        }
    }
}
