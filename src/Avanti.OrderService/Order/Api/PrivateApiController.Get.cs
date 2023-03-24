using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Avanti.OrderService.Order.Api;

public partial class PrivateApiController
{
    [SwaggerResponse(200, "The order is found")]
    [SwaggerResponse(400, "The input is invalid")]
    [SwaggerResponse(404, "The order is not found")]
    [SwaggerOperation(
                Summary = "Get a order",
                Description = "Retrieves a order by identifier.",
                Tags = new[] { "Order" })]
    [HttpGet("{Id}")]
    public async Task<IActionResult> GetOrder([FromRoute] GetOrderRequest request)
    {
        this.logger.LogDebug($"Incoming request to get order with id '{request.Id}'");

        return await this.orderActorRef.Ask<OrderActor.IResponse>(
            new OrderActor.GetOrderById { Id = request.Id }) switch
        {
            OrderActor.OrderFound found => new OkObjectResult(this.mapper.Map<GetOrderResponse>(found)),
            OrderActor.OrderNotFound => new NotFoundResult(),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }
}
