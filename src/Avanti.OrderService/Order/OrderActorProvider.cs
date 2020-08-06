using Akka.Actor;
using Akka.DI.Core;
using Avanti.Core.Microservice.Actors;

namespace Avanti.OrderService.Order
{
    public class OrderActorProvider : BaseActorProvider<OrderActor>
    {
        public OrderActorProvider(ActorSystem actorRefFactory) =>
            ActorRef = actorRefFactory.ActorOf(actorRefFactory.DI().Props<OrderActor>(), "order-actor");
    }
}
