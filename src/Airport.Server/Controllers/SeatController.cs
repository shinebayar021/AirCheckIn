using Microsoft.AspNetCore.Mvc;
using Airport.Data;
using Airport.Business.DTOs;
using Airport.Hubs;
using Microsoft.AspNetCore.SignalR;
using Airport.Server.Services;
using Airport.Business.FlightServices.Interfaces;

namespace Airport.Server.Controllers
{
    // Seats controller
    [ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly AirportDbContext _context;
        private readonly IHubContext<StatusHub> _hubContext;

        public SeatsController(ISeatService seatService, IHubContext<StatusHub> hubContext)
        {
            _seatService = seatService;
            _hubContext = hubContext;
        }

        //Buh ongotsnii buh suudal
        [HttpGet]
        public async Task<IActionResult> GetSeats()
        {
            var seats = await _seatService.GetAllSeatsAsync();
            return Ok(seats);
        }

        //Suudlaa nislegeer ni shuuj gargah
        [HttpGet("by-flight/{flightNumber}")]
        public async Task<IActionResult> GetSeatsByFlight(string flightNumber)
        {
            var seats = await _seatService.GetSeatsByFlightNumberAsync(flightNumber);
            if (seats == null)
                return NotFound($"Nisleg oldsongui esvel suudal oldsongui {flightNumber}");

            return Ok(seats);
        }

        //Suudal ezlegdsen bolgoh (check in)
        [HttpPut("seat-status")]
        public async Task<IActionResult> UpdateSeat([FromBody] SeatUpdateDto updatedSeat)
        {
            var (seat, error) = await _seatService.UpdateSeatStatusAsync(updatedSeat);
            if (error != null)
                return error == "Bad request..." ? BadRequest(error) : Conflict(error);
            if (seat != null)
            {
                var payload = $"Nisleg {seat.FlightId} deer {seat.SeatNumber} suudal songogdloo.";
                await WebSocketServerService.BroadcastAsync(payload);
            }
            return Ok(seat);
        }


        // Checked in? GET
        [HttpGet("checked-in/{passengerId}")]
        public async Task<IActionResult> HasPassengerCheckedIn(int passengerId)
        {
            var checkedIn = await _seatService.HasPassengerCheckedInAsync(passengerId);
            return Ok(checkedIn);
        }
    }
}
