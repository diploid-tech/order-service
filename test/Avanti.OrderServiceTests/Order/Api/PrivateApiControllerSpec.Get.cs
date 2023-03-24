using System;
using System.Globalization;
using Avanti.OrderService.Order;
using Avanti.OrderService.Order.Api;
using Avanti.OrderService.Order.Documents;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Avanti.OrderServiceTests.Order.Api;

public partial class PrivateApiControllerSpec
{
    public class When_GetOrder_Request_Is_Received : PrivateApiControllerSpec
    {
        [Fact]
        public async void Should_Return_200_When_Found()
        {
            var order = new OrderDocument
            {
                OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                Lines = new[]
                {
                    new OrderDocument.OrderLine { ProductId = 5, Amount = 1 },
                    new OrderDocument.OrderLine { ProductId = 7, Amount = 5 }
                }
            };

            progOrderActor.SetResponseForRequest<OrderActor.GetOrderById>(request =>
                new OrderActor.OrderFound { Id = 501, Document = order });

            IActionResult result = await Subject.GetOrder(
                new PrivateApiController.GetOrderRequest { Id = 501 });

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(
                    new PrivateApiController.GetOrderResponse
                    {
                        Id = 501,
                        OrderDate = DateTimeOffset.Parse("2020-07-01T19:00:00Z", CultureInfo.InvariantCulture),
                        Lines = new[]
                        {
                            new PrivateApiController.GetOrderResponse.OrderLine { ProductId = 5, Amount = 1 },
                            new PrivateApiController.GetOrderResponse.OrderLine { ProductId = 7, Amount = 5 }
                        }
                    });

            progOrderActor.GetRequest<OrderActor.GetOrderById>()
                .Should().BeEquivalentTo(
                    new OrderActor.GetOrderById { Id = 501 });
        }

        [Fact]
        public async void Should_Return_404_When_Not_Found()
        {
            progOrderActor.SetResponseForRequest<OrderActor.GetOrderById>(request =>
                new OrderActor.OrderNotFound());

            IActionResult result = await Subject.GetOrder(
                new PrivateApiController.GetOrderRequest { Id = 501 });

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async void Should_Return_500_When_Failure_To_Retrieve()
        {
            progOrderActor.SetResponseForRequest<OrderActor.GetOrderById>(request =>
                new OrderActor.OrderRetrievalFailed());

            IActionResult result = await Subject.GetOrder(
                new PrivateApiController.GetOrderRequest { Id = 501 });

            result.Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
