using AutoMapper;
using Avanti.Core.Microservice.Actors;
using Avanti.Core.Unittests;
using Avanti.OrderService.Order;
using Avanti.OrderService.Order.Api;
using Avanti.OrderService.Order.Mappings;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Avanti.OrderServiceTests.Order.Api;

public partial class PrivateApiControllerSpec : WithSubject<PrivateApiController>
{
    private readonly ProgrammableActor<OrderActor> progOrderActor;

    private PrivateApiControllerSpec()
    {
        progOrderActor = Kit.CreateProgrammableActor<OrderActor>("order-actor");
        IActorProvider<OrderActor> orderActorProvider = An<IActorProvider<OrderActor>>();
        orderActorProvider.Get().Returns(progOrderActor.TestProbe);

        var config = new MapperConfiguration(cfg => cfg.AddProfile(new OrderMapping()));
        config.AssertConfigurationIsValid();

        Subject = new PrivateApiController(
            orderActorProvider,
            An<ILogger<PrivateApiController>>(),
            config.CreateMapper());
    }
}
