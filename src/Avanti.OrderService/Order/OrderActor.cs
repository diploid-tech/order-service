using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using AutoMapper;
using Avanti.Core.EventStream;
using Avanti.Core.Microservice;
using Avanti.Core.RelationalData;
using Avanti.Core.RelationalData.PostgreSQL;
using Avanti.OrderService.Order.Documents;

namespace Avanti.OrderService.Order
{
    public partial class OrderActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly IRelationalDataStoreActorProvider relationalDataStoreActorProvider;
        private readonly IPlatformEventActorProvider platformEventActorProvider;
        private readonly IMapper mapper;
        private readonly IClock clock;

        public OrderActor(
            IRelationalDataStoreActorProvider datastoreActorProvider,
            IPlatformEventActorProvider platformEventActorProvider,
            IMapper mapper,
            IClock clock)
        {
            this.relationalDataStoreActorProvider = datastoreActorProvider;
            this.platformEventActorProvider = platformEventActorProvider;
            this.mapper = mapper;
            this.clock = clock;

            ReceiveAsync<GetOrderById>(m => Handle(m).PipeTo(Sender));
            ReceiveAsync<InsertExternalOrder>(m => Handle(m).PipeTo(Sender));
        }

        private async Task<IResponse> Handle(GetOrderById m)
        {
            this.log.Info($"Incoming request for getting order by id {m.Id}");

            var result = await this.relationalDataStoreActorProvider.ExecuteScalarJsonAs<OrderDocument>(
                DataStoreStatements.GetOrderById,
                new
                {
                    Id = m.Id
                });

            return result switch
            {
                IsSome<OrderDocument> scalar => new OrderFound { Id = m.Id, Document = scalar.Value },
                IsNone _ => new OrderNotFound(),
                _ => new OrderRetrievalFailed()
            };
        }

        private async Task<IResponse> Handle(InsertExternalOrder m)
        {
            this.log.Info($"Incoming request to upsert order with external id '{m.ExternalId}' from system '{m.System}'");

            var existingOrder = await GetExistingOrderByExternalIdentifier(m.ExternalId, m.System);
            if (existingOrder is IsSome<int>)
            {
                this.log.Warning($"Existing order with external id '{m.ExternalId}' from system '{m.System}' is tried to insert again. Skipping...");
                return new OrderAlreadyExists();
            }

            if (existingOrder is IsFailure)
            {
                this.log.Error($"Could not process order with external id '{m.ExternalId}' from system '{m.System}'");
                return new OrderFailedToStore();
            }

            var document = mapper.Map<OrderDocument>(m);
            var result = await this.relationalDataStoreActorProvider.ExecuteScalar<int>(
                DataStoreStatements.InsertOrder,
                new
                {
                    OrderJson = JsonSerializer.Serialize(document),
                    Now = this.clock.Now()
                });

            switch (result)
            {
                case IsSome<int> id:
                    var e = mapper.Map<Events.OrderInserted>((id.Value, document));
                    if (await platformEventActorProvider.SendEvent(e) is IsSuccess)
                    {
                        return new OrderInserted { Id = id.Value };
                    }

                    break;

                default:
                    break;
            }

            return new OrderFailedToStore();
        }

        private async Task<Result<int>> GetExistingOrderByExternalIdentifier(string externalId, string system) =>
            await this.relationalDataStoreActorProvider.ExecuteScalar<int>(
                DataStoreStatements.GetOrderByExternalId,
                new
                {
                    JsonValue = new JsonBParameter($"{{\"{system}\": \"{externalId}\"}}")
                });
    }
}