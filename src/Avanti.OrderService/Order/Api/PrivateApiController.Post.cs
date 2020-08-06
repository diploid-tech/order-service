using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Avanti.OrderService.Order.Api
{
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
            await orderActorRef.Ask<OrderActor.IResponse>(
                mapper.Map<OrderActor.InsertExternalOrder>(request)) switch
            {
                OrderActor.OrderInserted stored => new OkObjectResult(new PostOrderResponse
                {
                    Id = stored.Id
                }),
                OrderActor.OrderAlreadyExists _ => new ConflictResult(),
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
    }
}
