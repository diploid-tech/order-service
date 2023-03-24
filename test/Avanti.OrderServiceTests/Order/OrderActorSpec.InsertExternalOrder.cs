using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using Akka.Actor;
using Avanti.Core.EventStream;
using Avanti.Core.RelationalData;
using Avanti.Core.RelationalData.PostgreSQL;
using Avanti.OrderService.Order;
using Avanti.OrderService.Order.Documents;
using Avanti.OrderService.Order.Events;
using FluentAssertions;
using Xunit;

namespace Avanti.OrderServiceTests.Order;

public partial class OrderActorSpec
{
    public class When_Insert_External_Order_Is_Received : OrderActorSpec
    {
        private readonly OrderActor.InsertExternalOrder input = new()
        {
            ExternalId = "53419-01",
            System = "eCommerceSystem",
            OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
            Lines = new[]
            {
                new OrderActor.InsertExternalOrder.OrderLine { ProductId = 5, Amount = 1 },
                new OrderActor.InsertExternalOrder.OrderLine { ProductId = 7, Amount = 5 }
            }
        };

        public When_Insert_External_Order_Is_Received()
        {
            progDatastoreActor.SetResponseForRequest<RelationalDataStoreActor.ExecuteScalar>(request =>
                request.SqlCommand == DataStoreStatements.GetOrderByExternalId ?
                    new RelationalDataStoreActor.ScalarResult(null) :
                    new RelationalDataStoreActor.ScalarResult(501));

            progPlatformEventActor.SetResponseForRequest<PlatformEventActor.SendEvent>(request =>
                new PlatformEventActor.EventSendSuccess());
        }

        [Fact]
        public void Should_Return_Stored_When_Order_Is_Stored_Successfully_And_Send_Event()
        {
            var document = new OrderDocument
            {
                OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                Lines = new[]
                {
                    new OrderDocument.OrderLine { ProductId = 5, Amount = 1 },
                    new OrderDocument.OrderLine { ProductId = 7, Amount = 5 }
                },
                ExternalIdentifiers = new Dictionary<string, string>
                {
                    { "eCommerceSystem", "53419-01" }
                }.ToImmutableDictionary()
            };

            Subject.Tell(input);

            Kit.ExpectMsg<OrderActor.OrderInserted>().Should().BeEquivalentTo(
                new OrderActor.OrderInserted
                {
                    Id = 501
                });

            IEnumerable<RelationalDataStoreActor.ExecuteScalar> r = progDatastoreActor.GetRequests<RelationalDataStoreActor.ExecuteScalar>();
            progDatastoreActor.GetRequests<RelationalDataStoreActor.ExecuteScalar>()
                .Should().BeEquivalentTo(new[]
                {
                    new RelationalDataStoreActor.ExecuteScalar(
                        DataStoreStatements.GetOrderByExternalId,
                        new
                        {
                            JsonValue = new JsonBParameter($"{{\"eCommerceSystem\": \"53419-01\"}}")
                        }),
                    new RelationalDataStoreActor.ExecuteScalar(
                        DataStoreStatements.InsertOrder,
                        new
                        {
                            OrderJson = JsonSerializer.Serialize(document),
                            Now = DateTimeOffset.Parse("2018-04-01T07:00:00Z", CultureInfo.InvariantCulture)
                        })
                });

            progPlatformEventActor.GetRequest<PlatformEventActor.SendEvent>().Should().BeEquivalentTo(
                new PlatformEventActor.SendEvent(
                    new OrderInserted
                    {
                        Id = 501,
                        OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture)
                    }));
        }

        [Fact]
        public void Should_Return_Failure_When_Failed_To_Store()
        {
            progDatastoreActor.SetResponseForRequest<RelationalDataStoreActor.ExecuteScalar>(request =>
                new RelationalDataStoreActor.ExecuteFailed());

            Subject.Tell(input);

            Kit.ExpectMsg<OrderActor.OrderFailedToStore>();
        }

        [Fact]
        public void Should_Return_Failure_When_Failed_To_Send_Event()
        {
            progPlatformEventActor.SetResponseForRequest<PlatformEventActor.SendEvent>(request =>
                new PlatformEventActor.EventSendFailed());

            Subject.Tell(input);

            Kit.ExpectMsg<OrderActor.OrderFailedToStore>();
        }
    }
}
