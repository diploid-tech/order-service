using System;
using System.Globalization;
using Avanti.OrderService.Order;
using Avanti.OrderService.Order.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Avanti.OrderServiceTests.Order.Api
{
    public partial class PrivateApiControllerSpec
    {
        public class When_PostOrder_Request_Is_Received : PrivateApiControllerSpec
        {
            private PrivateApiController.PostOrderRequest request = new PrivateApiController.PostOrderRequest
            {
                ExternalId = "53419-01",
                System = "eCommerceSystem",
                OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                Lines = new[]
                {
                    new PrivateApiController.PostOrderRequest.OrderLine { ProductId = 5, Amount = 1 },
                    new PrivateApiController.PostOrderRequest.OrderLine { ProductId = 7, Amount = 5 }
                }
            };

            [Fact]
            public async void Should_Return_200_When_Stored()
            {
                progOrderActor.SetResponseForRequest<OrderActor.InsertExternalOrder>(request =>
                    new OrderActor.OrderInserted { Id = 500 });

                var result = await Subject.PostOrder(request);

                result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeEquivalentTo(new PrivateApiController.PostOrderResponse { Id = 500 });

                progOrderActor.GetRequest<OrderActor.InsertExternalOrder>()
                    .Should().BeEquivalentTo(
                        new OrderActor.InsertExternalOrder
                        {
                            ExternalId = "53419-01",
                            System = "eCommerceSystem",
                            OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                            Lines = new[]
                            {
                                new OrderActor.InsertExternalOrder.OrderLine { ProductId = 5, Amount = 1 },
                                new OrderActor.InsertExternalOrder.OrderLine { ProductId = 7, Amount = 5 }
                            }
                        });
            }

            [Fact]
            public async void Should_Return_500_When_Failed_To_Store()
            {
                progOrderActor.SetResponseForRequest<OrderActor.InsertExternalOrder>(request =>
                    new OrderActor.OrderFailedToStore());

                var result = await Subject.PostOrder(request);

                result.Should().BeOfType<StatusCodeResult>()
                    .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            }

            [Fact]
            public async void Should_Return_409_When_Already_Exists()
            {
                progOrderActor.SetResponseForRequest<OrderActor.InsertExternalOrder>(request =>
                    new OrderActor.OrderAlreadyExists());

                var result = await Subject.PostOrder(request);

                result.Should().BeOfType<ConflictResult>();
            }
        }
    }
}
