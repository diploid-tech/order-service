using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Avanti.OrderService.Order.Api;

public partial class PrivateApiController
{
    [SwaggerResponse(200, "The order is stored")]
    [SwaggerResponse(400, "The order is invalid")]
    [SwaggerOperation(
                Summary = "Upsert a order",
                Description = "Insert or update the given order, identified by id.",
                Tags = new[] { "Order" })]
    [HttpPost]
    public async Task<IActionResult> PostOrder([FromBody] PostOrderRequest request) =>
        await this.orderActorRef.Ask<OrderActor.IResponse>(
            this.mapper.Map<OrderActor.InsertExternalOrder>(request)) switch
        {
            OrderActor.OrderInserted stored => new OkObjectResult(new PostOrderResponse
            {
                Id = stored.Id
            }),
            OrderActor.OrderAlreadyExists => new ConflictResult(),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
}
