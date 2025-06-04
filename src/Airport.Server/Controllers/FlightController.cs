using Microsoft.AspNetCore.Mvc;
using Airport.Data.Models;
using Airport.Hubs;
using Microsoft.AspNetCore.SignalR;
using Airport.Business.FlightServices.Interfaces;
using Airport.Business.Services.Interfaces;

namespace Airport.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;
        //IHubContext<SeatsHub> interface ni serverees client ruu SignalR Hub-aar damjuulgan ilgeeh bolomj olgono
        private readonly IHubContext<StatusHub> _hubContext;

        public FlightsController(IFlightService flightService, IHubContext<StatusHub> hubContext)
        {
            _flightService = flightService;
            _hubContext = hubContext;
        }

        // All flights GET
        [HttpGet]
        public async Task<IActionResult> GetFlights()
        {
            var flights = await _flightService.GetAllFlightsAsync();
            return Ok(flights);
        }

        // Flight status UPDATE flightNumber buyu ongotsnii dugaaraar hain UPDATE hiih
        [HttpPut("update-status/{flightNumber}")]
        public async Task<IActionResult> UpdateFlightStatus(string flightNumber, [FromBody] FlightStatus newStatus)
        {
            // context-oos haih
            var flight = await _flightService.UpdateFlightStatusAsync(flightNumber, newStatus);
            if (flight == null)
                return NotFound(new { message = $" '{flightNumber}' oldsongu" });

            // Real-time ???????? ??????
            // Shinechlegdsen flight objectiig model baidlaar yvuulah
            var updatedDto = new
            {
                flight.FlightNumber,
                flight.Status,
                flight.DepartureTime,
            };

            // Client heseg ruu uurchlultuu yvulah "ReceiveFlightUpdate"-ni client taliin method ner
            await _hubContext.Clients.All.SendAsync("ReceiveFlightUpdate", updatedDto);
            return Ok(new { message = "Flight status uurchlugdsun." });
        }
    }



    //booking controller
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings);
        }
        //[HttpPut]
        //public async Task<IActionResult> UpdateBooking()
        //{
        //    return NotFound;
        //}
    }

}
