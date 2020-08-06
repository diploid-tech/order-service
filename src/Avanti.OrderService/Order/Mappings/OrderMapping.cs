using System.Collections.Generic;
using System.Collections.Immutable;
using AutoMapper;
using Avanti.OrderService.Order.Api;
using Avanti.OrderService.Order.Documents;
using Avanti.OrderService.Order.Events;

namespace Avanti.OrderService.Order.Mappings
{
    public class OrderMapping : Profile
    {
        public OrderMapping()
        {
            CreateMap<PrivateApiController.PostOrderRequest, OrderActor.InsertExternalOrder>();
            CreateMap<PrivateApiController.PostOrderRequest.OrderLine, OrderActor.InsertExternalOrder.OrderLine>();
            CreateMap<OrderActor.OrderFound, PrivateApiController.GetOrderResponse>()
                .AfterMap((src, dest, context) => context.Mapper.Map(src.Document, dest))
                .ForMember(s => s.Id, o => o.MapFrom(s => s.Id))
                .ForAllOtherMembers(o => o.Ignore());
            CreateMap<OrderDocument.OrderLine, PrivateApiController.GetOrderResponse.OrderLine>();
            CreateMap<OrderDocument, PrivateApiController.GetOrderResponse>()
                .ForMember(s => s.Id, o => o.Ignore());
            CreateMap<OrderActor.InsertExternalOrder, OrderDocument>()
                .ForMember(s => s.ExternalIdentifiers, o => o.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    dest.ExternalIdentifiers = new Dictionary<string, string> { { src.System, src.ExternalId } }.ToImmutableDictionary();
                });
            CreateMap<OrderActor.InsertExternalOrder.OrderLine, OrderDocument.OrderLine>();
            CreateMap<(int Id, OrderDocument Document), OrderInserted>()
                .ForMember(s => s.Id, o => o.MapFrom(s => s.Id))
                .ForMember(s => s.OrderDate, o => o.MapFrom(s => s.Document.OrderDate));
        }
    }
}
