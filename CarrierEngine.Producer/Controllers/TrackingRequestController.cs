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
    public class TrackingRequestController : ControllerBase
    {
        private readonly IBus _bus;
        private readonly ILogger<TrackingRequestController> _logger;

        public TrackingRequestController(IBus bus, ILogger<TrackingRequestController> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(TrackingRequestDto trackingRequestDto)
        {
            if (trackingRequestDto is null)
            {
                return BadRequest();
            }

            try
            {
                await QueueHelper.SentToMessageQueue(trackingRequestDto, RabbitMqConstants.TrackingRequestQueue, _bus, trackingRequestDto.CorrelationId, trackingRequestDto.BanyanLoadId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send {@trackingRequestDto} to {RabbitMqConstants.TrackingRequestQueue}", trackingRequestDto, RabbitMqConstants.TrackingRequestQueue);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to send message to queue.");
            }

            return Ok();
        }



        /// <summary>
        /// Used for manually tracking a shipment. 
        /// </summary>
        /// <param name="trackingRequestDto"></param>
        /// <returns></returns>
        [HttpPost("manual")]
        public async Task<IActionResult> ManualTrack(TrackingRequestDto trackingRequestDto)
        {
            if (trackingRequestDto is null)
            {
                return BadRequest();
            }

            throw new Exception("test");


            try
            {
                await Task.Delay(DateTime.Now.Second % 2 == 0 ? 4000 : 2000);

                return Ok(new { Id = Guid.NewGuid() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send {@trackingRequestDto} to {RabbitMqConstants.TrackingRequestQueue}", trackingRequestDto, RabbitMqConstants.TrackingRequestQueue);
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to send message to queue.");
            }


        }
    }
}