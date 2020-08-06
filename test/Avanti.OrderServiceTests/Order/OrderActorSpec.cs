using System;
using System.Globalization;
using Akka.Actor;
using AutoMapper;
using Avanti.Core.EventStream;
using Avanti.Core.Microservice;
using Avanti.Core.Microservice.Middleware;
using Avanti.Core.RelationalData;
using Avanti.Core.Unittests;
using Avanti.OrderService.Order;
using Avanti.OrderService.Order.Mappings;
using NSubstitute;

namespace Avanti.OrderServiceTests.Order
{
    public partial class OrderActorSpec : WithSubject<IActorRef>
    {
        private ProgrammableActor<RelationalDataStoreActor> progDatastoreActor;
        private ProgrammableActor<PlatformEventActor> progPlatformEventActor;

        private OrderActorSpec()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new OrderMapping());
            });
            config.AssertConfigurationIsValid();

            this.progDatastoreActor = Kit.CreateProgrammableActor<RelationalDataStoreActor>();
            var relationalDataStoreActorProvider = new RelationalDataStoreActorProvider(this.progDatastoreActor.TestProbe);

            this.progPlatformEventActor = Kit.CreateProgrammableActor<PlatformEventActor>();
            var platformEventActorProvider = new PlatformEventActorProvider(this.progPlatformEventActor.TestProbe);

            var clock = new FakeClock(DateTimeOffset.Parse("2018-04-01T07:00:00Z", CultureInfo.InvariantCulture));

            Subject = Sys.ActorOf(
                Props.Create<OrderActor>(
                    relationalDataStoreActorProvider,
                    platformEventActorProvider,
                    config.CreateMapper(),
                    clock));
        }
    }
}
