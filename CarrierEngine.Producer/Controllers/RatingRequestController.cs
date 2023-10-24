using System;
using System.Net;
using System.Threading.Tasks;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Dtos;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingRequestController : ControllerBase
    {
        private readonly IBus _bus;
        private readonly ILogger<RatingRequestController> _logger;

        public RatingRequestController(IBus bus, ILogger<RatingRequestController> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(RatingRequestDto ratingRequestDto)
        {
            if (ratingRequestDto is null)
            {
                return BadRequest();
            }

            try
            {
                await QueueHelper.SentToMessageQueue<RatingRequestDto>(ratingRequestDto, RabbitMqConstants.RatingRequestQueue, _bus, NewId.NextGuid());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send {@ratingRequestDto} to {RabbitMqConstants.RatingRequestQueue}", ratingRequestDto, RabbitMqConstants.RatingRequestQueue);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to send message to queue.");
            }

            return Ok();
        }

    }
}