using AutoMapper;
using Avanti.Core.Microservice.Actors;
using Microsoft.AspNetCore.Mvc;

namespace Avanti.OrderService.Order.Api;

[Route("/private/order")]
[ApiController]
public partial class PrivateApiController
{
    private readonly IMapper mapper;
    private readonly IActorRef orderActorRef;
    private readonly ILogger logger;

    public PrivateApiController(
        IActorProvider<OrderActor> orderActorProvider,
        ILogger<PrivateApiController> logger,
        IMapper mapper)
    {
        this.orderActorRef = orderActorProvider.Get();
        this.logger = logger;
        this.mapper = mapper;
    }
}
