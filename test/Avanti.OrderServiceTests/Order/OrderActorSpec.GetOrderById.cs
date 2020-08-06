using System;
using System.Globalization;
using System.Text.Json;
using Akka.Actor;
using Avanti.Core.RelationalData;
using Avanti.OrderService.Order;
using Avanti.OrderService.Order.Documents;
using FluentAssertions;
using Xunit;

namespace Avanti.OrderServiceTests.Order
{
    public partial class OrderActorSpec
    {
        public class When_Get_Order_By_Id_Is_Received : OrderActorSpec
        {
            private OrderActor.GetOrderById input = new OrderActor.GetOrderById
            {
                Id = 501
            };

            [Fact]
            public void Should_Return_Order_When_Found()
            {
                var document = new OrderDocument
                {
                    OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                    Lines = new[]
                    {
                        new OrderDocument.OrderLine { ProductId = 5, Amount = 1 },
                        new OrderDocument.OrderLine { ProductId = 7, Amount = 5 }
                    }
                };

                this.progDatastoreActor.SetResponseForRequest<RelationalDataStoreActor.ExecuteScalar>(request =>
                        new RelationalDataStoreActor.ScalarResult(JsonSerializer.Serialize(document)));

                Subject.Tell(input);

                Kit.ExpectMsg<OrderActor.OrderFound>().Should().BeEquivalentTo(
                    new OrderActor.OrderFound
                    {
                        Id = 501,
                        Document = document
                    });

                this.progDatastoreActor.GetRequest<RelationalDataStoreActor.ExecuteScalar>()
                    .Should().BeEquivalentTo(new RelationalDataStoreActor.ExecuteScalar(
                        DataStoreStatements.GetOrderById,
                        new
                        {
                            Id = 501
                        }));
            }

            [Fact]
            public void Should_Return_Not_Found_When_Not_Found()
            {
                this.progDatastoreActor.SetResponseForRequest<RelationalDataStoreActor.ExecuteScalar>(request =>
                    new RelationalDataStoreActor.ScalarResult(null));

                Subject.Tell(input);

                Kit.ExpectMsg<OrderActor.OrderNotFound>();
            }

            [Fact]
            public void Should_Return_Failure_When_Could_Not_Retrieve_Data()
            {
                this.progDatastoreActor.SetResponseForRequest<RelationalDataStoreActor.ExecuteScalar>(request =>
                    new RelationalDataStoreActor.ExecuteFailed());

                Subject.Tell(input);

                Kit.ExpectMsg<OrderActor.OrderRetrievalFailed>();
            }
        }
    }
}
