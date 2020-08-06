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
            logger.LogDebug($"Incoming request to get order with id '{request.Id}'");

            return await orderActorRef.Ask<OrderActor.IResponse>(
                new OrderActor.GetOrderById { Id = request.Id }) switch
            {
                OrderActor.OrderFound found => new OkObjectResult(mapper.Map<GetOrderResponse>(found)),
                OrderActor.OrderNotFound _ => new NotFoundResult(),
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
